using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
    public class AbonoModel : BaseNopModel
    {
        public ClienteModel Receptor { get; set; }

        //Indicador de la cuenta a abonar propia o tercero
        [NopResourceDisplayName("Account.Fields.OwnerBalance")]
        public int IndCuentaCliente { get; set; }

        //Id de transaccion de paypal
        [NopResourceDisplayName("Account.Fields.TransaccionPayPalId")]
        public string TransaccionPayPalId { get; set; }

        [NopResourceDisplayName("Account.Fields.TransactionAmount")]
        public string MontoTransaccion { get; set; }

        // se usa para indicar si se abonara en Giftcard
        [NopResourceDisplayName("Account.Fields.IndGiftCard")]
        public string IndGiftCard { get; set; }
        
        /// <summary>
        /// Codigo del abono en crm. 
        /// </summary>
        public long Cod_Abono { get; set; }

        /// <summary>
        /// Codigo del GiftCard obtenido de crm. 
        /// </summary>
        public string Cod_GiftCard { get; set; }

        [NopResourceDisplayName("Account.Fields.MontoTransaccionGiftCard")]
        public string MontoTransaccionGiftCard { get; set; }
    }
}
