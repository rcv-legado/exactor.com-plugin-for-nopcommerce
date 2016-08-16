using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Tax.Exactor.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Tax.Exactor.Controllers
{
    public class TaxExactorController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;

        public TaxExactorController(IWorkContext workContext,
            IStoreService storeService,
            ITaxService taxService,
            ISettingService settingService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._taxService = taxService;
            this._settingService = settingService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._localizationService = localizationService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var exactorTaxSettings = _settingService.LoadSetting<ExactorTaxSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MerchantId = exactorTaxSettings.MerchantId,
                UserId = exactorTaxSettings.UserId,
                ActiveStoreScopeConfiguration = storeScope
            };

            model.TestAddress.AvailableCountries = _countryService.GetAllCountries(showHidden: true)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            model.TestAddress.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            if (model.TestAddress.CountryId != 0)
                model.TestAddress.AvailableStates = _stateProvinceService.GetStateProvincesByCountryId(model.TestAddress.CountryId, showHidden: true)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            model.TestAddress.AvailableStates.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View("~/Plugins/Tax.Exactor/Views/TaxExactor/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var exactorTaxSettings = _settingService.LoadSetting<ExactorTaxSettings>(storeScope);

            exactorTaxSettings.MerchantId = model.MerchantId;
            exactorTaxSettings.UserId = model.UserId;
            _settingService.SaveSetting(exactorTaxSettings);
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("test")]
        public ActionResult TestRequest(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            var country = _countryService.GetCountryById(model.TestAddress.CountryId);
            var sateProvince = _stateProvinceService.GetStateProvinceById(model.TestAddress.RegionId);

            var taxProvider = (ExactorTaxProvider)_taxService.LoadTaxProviderBySystemName("Tax.Exactor");

            if (taxProvider == null)
            {
                ErrorNotification("Can't load tax provider");
                return Configure();
            }

            var address = new Address
            {
                Country = country,
                StateProvince = sateProvince,
                City = model.TestAddress.City,
                Address1 = model.TestAddress.Address,
                ZipPostalCode = model.TestAddress.ZipPostalCode,
                FirstName = "nopCommerce",
                LastName = "Test"
            };

            var taxResult = taxProvider.GetTaxRate(new CalculateTaxRequest { Address = address });

            if (taxResult.Success)
            {
                SuccessNotification(String.Format(_localizationService.GetResource("Plugins.Tax.Exactor.TestSuccess"), taxResult.TaxRate));
            }
            else
            {
                ErrorNotification(taxResult.Errors.Aggregate((all, curent) => all + "\r\n" + curent));
            }

            return Configure();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStatesByCountryId(int countryId)
        {
            var states = _stateProvinceService.GetStateProvincesByCountryId(countryId, showHidden: true)
                .Select(x => new { id = x.Id, name = x.Name }).ToList();
            if (states.Count == 0)
                states.Insert(0, new { id = 0, name = _localizationService.GetResource("Admin.Address.OtherNonUS") });
            return Json(states, JsonRequestBehavior.AllowGet);
        }
    }
}