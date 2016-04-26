using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web.Routing;
using System.Xml.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Plugins;
using Nop.Plugin.Tax.Exactor.Infrastructure.Cache;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tax;

namespace Nop.Plugin.Tax.Exactor
{
    /// <summary>
    /// Exaxtor tax provider
    /// </summary>
    public class ExactorTaxProvider:BasePlugin, ITaxProvider
    {
		private readonly ISettingService _settingService;
        private readonly ExactorTaxSettings _exactorTaxSettings;
        private readonly ICacheManager _cacheManager;
	    private readonly ITaxCategoryService _taxCategoryService;
        private const string REQUEST_URL = "https://taxrequest.exactor.com/request/xml";

        public ExactorTaxProvider(ISettingService settingService,
            ExactorTaxSettings exactorTaxSettings,
            ICacheManager cacheManager,
            ITaxCategoryService taxCategoryService)
        {
            this._settingService = settingService;
            this._exactorTaxSettings = exactorTaxSettings;
            this._cacheManager = cacheManager;
            this._taxCategoryService = taxCategoryService;
        }

	    /// <summary>
	    /// Gets tax rate
	    /// </summary>
	    /// <param name="calculateTaxRequest">Tax calculation request</param>
	    /// <returns>Tax</returns>
	    public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
	    {
	        var address = calculateTaxRequest.Address;
	        if (address == null || address.Country==null)
	            return new CalculateTaxResult {Errors = new List<string> {"Address is not set"}};

            var errors = new List<string>();

            var taxRate = _cacheManager.Get<decimal>(string.Format(ModelCacheEventConsumer.TAXRATE_KEY, address.Id, calculateTaxRequest.TaxCategoryId), () =>
	            {
                    var tax = decimal.Zero;

                    var taxCategory = _taxCategoryService.GetTaxCategoryById(calculateTaxRequest.TaxCategoryId);

                    var taxCategoryName = taxCategory == null ? "Anything" : taxCategory.Name;
                    taxCategoryName = XmlHelper.XmlEncode(taxCategoryName);

                    var fullName = string.Format("{0} {1}", address.FirstName, address.LastName);

                    //create tax request
                    var xml = String.Format(Properties.Resources.taxRequest,
                        _exactorTaxSettings.MerchantId,
                        _exactorTaxSettings.UserId,
                        fullName,
                        address.Address1,
                        address.Address2 ?? String.Empty,
                        address.City,
                        address.StateProvince != null ? address.StateProvince.Name : String.Empty,
                        address.ZipPostalCode,
                        address.Country.Name,
                        taxCategoryName, //description
                        100, //gross amount
                        DateTime.Now.ToString("yyyy-MM-dd") //sale date
                        );

                    string data;
                    using (var client = new WebClient())
                    {
                        try
                        {
                            data = client.UploadString(REQUEST_URL, xml);
                        }
                        catch (WebException ex)
                        {
                            
                           errors.Add(ex.Message);
                            return 0;
                        }
                        
                    }

                    var taxResponse = XDocument.Parse(data).Root;

	                if (taxResponse == null)
	                    return 0;

                    //get XML namespace
                    var ns = taxResponse.Name.ToString().Replace("TaxResponse", "");

                    var invoiceResponse = taxResponse.Element(ns + "InvoiceResponse");
                    var errorResponse = taxResponse.Element(ns + "ErrorResponse");

	                if (invoiceResponse == null && errorResponse == null)
	                    return 0;

                    if (errorResponse != null)
                    {
                        errors.Add(String.Format("Line {0}: {1}", errorResponse.Element(ns + "LineNumber").Value, errorResponse.Element(ns + "ErrorDescription").Value));
                    }
                    else
                    {
                        tax = Convert.ToDecimal(invoiceResponse.Element(ns + "TotalTaxAmount").Value, new CultureInfo("en-US"));
                    }

	                return tax;
	            });

            return new CalculateTaxResult {Errors = errors, TaxRate = taxRate};
	    }

	    /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TaxExactor";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Tax.Exactor.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new ExactorTaxSettings
            {
                MerchantId = string.Empty,
                UserId = string.Empty
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.MerchantId", "Merchant ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.MerchantId.Hint", "Enter the Merchant ID that you received during registration at the website exactor.com.");            
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.UserId", "User ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.UserId.Hint", "Enter your User ID from the website exactor.com.");
			this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Country", "Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Country.Hint", "Select country.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Region", "State/Province");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Region.Hint", "Select State/Province.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.City", "City");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.City.Hint", "Enter the city name.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Address", "Address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Address.Hint", "Enter the address.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.Test", "Test tax calculation");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.ZipPostalCode", "Zip postal code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.ZipPostalCode.Hint", "Enter the zip postal code.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Exactor.TestSuccess", "Test success! Tax: {0}");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ExactorTaxSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.UserId");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.UserId.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Country");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Country.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Region");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Region.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.City");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.City.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Address");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Address.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.Test");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.ZipPostalCode");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.ZipPostalCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Exactor.TestSuccess");

            base.Uninstall();
        }
    }
}
