using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Tax.Exactor.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            TestAddress = new AddressModel();
        }

        /// <summary>
        /// Merchant ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Tax.Exactor.MerchantId")]
        public string MerchantId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Tax.Exactor.UserId")]
        public string UserId { get; set; }

        /// <summary>
        /// Address for test
        /// </summary>
        public AddressModel TestAddress { get; set; }
    }
}
