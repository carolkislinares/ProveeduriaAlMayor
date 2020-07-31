using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
    public class TransaccionModel : BaseNopModel
    {
        /// <summary>
        /// Id de la operacion resultante de Paypal
        /// </summary>
        /// 
        [NopResourceDisplayName("Account.Fields.TransaccionPaypalID")]
        public string TransaccionPaypalID { get; set; }

        /// <summary>
        /// Id del abono que devuelve CRM
        /// </summary>
        /// 
        [NopResourceDisplayName("Account.Fields.TransaccionCreditID")]
        public long TransaccionCreditID { get; set; }

        /// <summary>
        /// Id Ecommerce del Emisor
        /// </summary>
        public int CustomerID { get; set; }

        /// <summary>
        /// Cedula del receptor
        /// </summary>
        public string CedulaReceptor { get; set; }

        /// <summary>
        /// Datos del Receptor del abono
        /// </summary>
        /// 
        [NopResourceDisplayName("Account.Fields.NombreReceptor")]
        public string NombreReceptor { get; set; }

        /// <summary>
        /// Monto del abono
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha de la operacion
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Estatus del abono dependiendo del resultado de la operacion CRM.
        /// </summary>
        public bool Estatus_Operacion { get; set; }

        /// <summary>
        /// Codigo del abono que devuelve CRM al completar la operacion.
        /// </summary>
        public long Cod_Abono { get; set; }

        /// <summary>
        ///   se usa para indicar si se abonara en Giftcard
        /// </summary>
        [NopResourceDisplayName("Account.Fields.IndGiftCard")]
        public bool IndGiftCard { get; set; }

        /// <summary>
        /// Codigo de GIFTCARD asignado en CRM para el cliente. 
        /// </summary>
        [NopResourceDisplayName("Account.Fields.CodigoGiftCard")]
        public string CodigoGiftCard { get; set; }


        public TransaccionModel(SigoCreditosInfoModel model)
        {
            TransaccionPaypalID = model.Abono.TransaccionPayPalId;
            TransaccionCreditID = model.Abono.Cod_Abono;
            CedulaReceptor = model.Abono.IndCuentaCliente == 1 ? model.Emisor.Documento : model.Abono.Receptor.Documento;
            Monto = Convert.ToDecimal(model.Abono.MontoTransaccion.Replace(".", string.Empty).Replace(",", ".").Trim());
            FechaCreacion = DateTime.Now;
            NombreReceptor = model.Abono.Receptor.Nombre + " " + model.Abono.Receptor.Apellido;
            CustomerID = model.Emisor.EcommerceId;
            IndGiftCard = Convert.ToBoolean(model.Abono.IndGiftCard);
            CodigoGiftCard = Convert.ToBoolean(model.Abono.IndGiftCard) ? model.Abono.Cod_GiftCard : string.Empty;
        }

        public TransaccionModel() { }

    }
}
