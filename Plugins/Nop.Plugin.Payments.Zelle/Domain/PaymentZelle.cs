using Nop.Core;

namespace Nop.Plugin.Payments.Zelle.Domain
{
    /// <summary>
    /// Represents a Bank
    /// </summary>
    public partial class PaymentZelle : BaseEntity
    {
        public string ReferenceNumber { get; set; }
        public int OrderId { get; set; }
        public string IssuingEmail { get; set; }
        public decimal OrderTotal { get; set; }
        public int PaymentStatusOrder { get; set; }
    }


}