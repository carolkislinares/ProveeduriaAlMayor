using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Zelle.Models
{
    public class PaymentInfoModel : BaseNopModel
    {

        [NopResourceDisplayName("Plugins.Payments.Zelle.Fields.IssuingEmail")]
        public string IssuingEmail { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Zelle.Fields.Reference")]
        public string Reference { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Zelle.Fields.Order")]
        public int Order { get; set; }

        public int PaymentStatusOrder { get; set; }
        public int IdZelle { get; set; }


    }
}