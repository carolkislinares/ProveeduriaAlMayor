using Nop.Core;

namespace Nop.Plugin.Payments.Transfer.Domain
{
    /// <summary>
    /// Represents a Bank
    /// </summary>
    public partial class Bank : BaseEntity
    {
        public string Name { get; set; }
        public bool IsReceiver { get; set; }
        public string AccountNumber { get; set; }
        public int StoreId { get; set; }
    }
}