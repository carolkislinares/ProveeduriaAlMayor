using Nop.Core;
using System;


namespace Nop.Plugin.Payments.ConsolidatePayment.Domain
{
    public partial class Consolidate : BaseEntity
    {
        public int TiendaId { get; set; }
        public int OrdenId { get; set; }
        public int ClienteId { get; set; }
        public int BancoEmisorId { get; set; }
        public int BancoReceptorId { get; set; }
        public int StatusPaymentOrder { get; set; }
        public string Referencia { get; set; }
        public string BancoEmisor { get; set; }
        public string BancoReceptor { get; set; }
        public string EmailEmisor { get; set; }
        public string Tienda { get; set; }
        public string MetodoPago { get; set; }
        public decimal MontoTotalOrden  { get; set; }
        public string CodigoMoneda { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }

    }
}
