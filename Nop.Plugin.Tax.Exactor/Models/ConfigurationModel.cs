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

        public int ActiveStoreScopeConfiguration { get; set; }

        /// <summary>
        /// Merchant ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Tax.Exactor.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantIdOverrideForStore { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Tax.Exactor.UserId")]
        public string UserId { get; set; }
        public bool UserIdOverrideForStore { get; set; }

        /// <summary>
        /// Address for test
        /// </summary>
        public AddressModel TestAddress { get; set; }
    }
}
