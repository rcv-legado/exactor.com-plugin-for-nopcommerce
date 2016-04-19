using Nop.Core.Configuration;

namespace Nop.Plugin.Tax.Exactor
{
	public class ExactorTaxSettings: ISettings
    {
        public string MerchantId { get; set; }

        public string UserId { get; set; } 	
	}
}
