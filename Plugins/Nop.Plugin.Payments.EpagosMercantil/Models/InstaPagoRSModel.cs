using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.EpagosMercantil.Models
{
    public class EpagosMercantilRSModel : BaseNopModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        
        public string Id { get; set; }

      
        public string Code { get; set; }

       
        public string Reference { get; set; }

        public string Voucher { get; set; }


        public string Ordernumber { get; set; }


        public string Sequence { get; set; }


        public string Approval { get; set; }


        public string Lote { get; set; }

        public string ResponseCode { get; set; }


        public string Deferred { get; set; }


        public string Datetime { get; set; }


        public string Amount { get; set; }
    }

    public class EpagosMercantilConfigurationModel : BaseNopModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }


        public string Id { get; set; }


        public string Code { get; set; }


        public string Reference { get; set; }

        public string Voucher { get; set; }


        public string Ordernumber { get; set; }


        public string Sequence { get; set; }


        public string Approval { get; set; }


        public string Lote { get; set; }

        public string ResponseCode { get; set; }


        public string Deferred { get; set; }


        public string Datetime { get; set; }


        public string Amount { get; set; }
    }
}