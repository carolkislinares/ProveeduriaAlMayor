using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.Transfer.Services;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.Transfer.Models
{
    /// <summary>
    /// Represents a PaymentTransferList
    /// </summary>
    public partial class PaymentTransferSearch
    {
        public PaymentTransferSearch()
        {
            BancoEmisor = new List<SelectListItem>();
            BancoReceptor = new List<SelectListItem>();
        }
       
        public int BancoEmisorId { get; set; }
        public IList<SelectListItem> BancoEmisor { get; set; }
        public int BancoReceptorId { get; set; }
        public IList<SelectListItem> BancoReceptor { get; set; }

        public int ID { get; set; }
        public string referencia { get; set; }
        public int orden { get; set; }
        public int PaymentStatus { get; set; }

       

    }

}