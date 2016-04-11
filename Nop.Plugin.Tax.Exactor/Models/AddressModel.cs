using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Tax.Exactor.Models
{
	public class AddressModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Tax.Exactor.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Exactor.Region")]
        public int RegionId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Exactor.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Exactor.Address")]
        public string Address { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Exactor.ZipPostalCode")]
        public string ZipPostalCode { get; set; }
    }
}
