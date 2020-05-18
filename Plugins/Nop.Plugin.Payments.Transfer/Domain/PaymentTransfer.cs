using Nop.Core;

namespace Nop.Plugin.Payments.Transfer.Domain
{
    /// <summary>
    /// Represents a PaymentTransfer
    /// </summary>
    public partial class PaymentTransfer : BaseEntity
    {
        public string ReferenceNumber { get; set; }
        public int OrderId { get; set; }
        public int ReceiverBankId { get; set; }
        public int IssuingBankId { get; set; }
        public string ReceiverBankName { get; set; }
        public string IssuingBankName { get; set; }
        public decimal OrderTotal { get; set; }
        public int PaymentStatusOrder { get; set; }
        
    }
}