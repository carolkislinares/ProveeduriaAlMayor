using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Transfer.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            BancoEmisor = new List<SelectListItem>();
            BancoReceptor = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Banco Emisor")]
        public string BancoEmisorId { get; set; }

        [NopResourceDisplayName("Banco Emisor")]
        public IList<SelectListItem> BancoEmisor { get; set; }

        [NopResourceDisplayName("Payment.Id")]
        public string BancoReceptorId { get; set; }

        [NopResourceDisplayName("Banco Emisor")]
        public IList<SelectListItem> BancoReceptor { get; set; }

        [NopResourceDisplayName("Referencia")]
        public string Referencia { get; set; }

        [NopResourceDisplayName("Nro. Orden")]
        public int NroOrden { get; set; }

        [NopResourceDisplayName("Id")]
        public int IdPayment { get; set; }

        [NopResourceDisplayName("Status Payment")]
        public int StatusPayment { get; set; }


    }
}