using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.ConsolidatePayment.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        //public int ActiveStoreScopeConfiguration { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.AdditionalFeePercentage")]
        //public bool AdditionalFeePercentage { get; set; }
        //public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.AdditionalFee")]
        //public decimal AdditionalFee { get; set; }
        //public bool AdditionalFee_OverrideForStore { get; set; }

        //public int TransactModeId { get; set; }
        //[NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.TransactMode")]
        //public SelectList TransactModeValues { get; set; }
        //public bool TransactModeId_OverrideForStore { get; set; }

        public ConfigurationModel()
        {
            MetodosPago = new List<SelectListItem>();
            BancosEmisores =   new List<SelectListItem>();
            BancosReceptores = new List<SelectListItem>();
            Tiendas = new List<SelectListItem>();
            StatusPaymentOrderList = new List<SelectListItem>();
        }

        public int Id { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.TiendaId")]
        public int TiendaId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.OrdenId")]
        public int OrdenId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.Orden")]
        public string Orden { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.ClienteId")]
        public int ClienteId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.BancoEmisorId")]
        public int BancoEmisorId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.BancoReceptorId")]
        public int BancoReceptorId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.StatusPaymentOrder")]
        public string StatusPaymentOrder { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.Referencia")]
        public string Referencia { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.BancoEmisor")]
        public string BancoEmisor { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.BancoReceptor")]
        public string BancoReceptor { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.EmailEmisor")]
        public string EmailEmisor { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.MetodoPago")]
        public string MetodoPago { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.Tienda")]
        public string Tienda { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.MontoTotalOrden")]
        public string MontoTotalOrden { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.CodigoMoneda")]
        public string CodigoMoneda { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.FechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ConsolidatePayment.Fields.UltimaActualizacion")]
        public DateTime FechaUltimaActualizacion { get; set; }

        public IList<SelectListItem> MetodosPago { get; set; }
        public IList<SelectListItem> BancosEmisores { get; set; }
        public IList<SelectListItem> BancosReceptores { get; set; }
        public IList<SelectListItem> Tiendas { get; set; }
        public IList<SelectListItem> StatusPaymentOrderList { get; set; }

    }
}