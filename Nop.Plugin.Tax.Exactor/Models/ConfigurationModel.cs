using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Tax.Exactor.Models
{
	/// <summary>
	/// Description of ConfigurationModel.
	/// </summary>
	public class ConfigurationModel:BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        /// <summary>
        /// Merchan ID
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
        
        public AddressModel TestAddress { get; set; }

        public string TestingResult { get; set; }
    }    
}
