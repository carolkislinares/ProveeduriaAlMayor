using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Zelle.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        //public int ActiveStoreScopeConfiguration { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.Zelle.Fields.AdditionalFeePercentage")]
        //public bool AdditionalFeePercentage { get; set; }
        //public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.Zelle.Fields.AdditionalFee")]
        //public decimal AdditionalFee { get; set; }
        //public bool AdditionalFee_OverrideForStore { get; set; }

        //public int TransactModeId { get; set; }
        //[NopResourceDisplayName("Plugins.Payments.Zelle.Fields.TransactMode")]
        //public SelectList TransactModeValues { get; set; }
        //public bool TransactModeId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.Zelle.Fields.IssuingEmail")]
        public int IssuingEmail { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Zelle.Fields.Reference")]
        public string Reference { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Zelle.Fields.Order")]
        public int Order { get; set; }

    }
}