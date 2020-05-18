using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.Transfer.Domain;

namespace Nop.Plugin.Payments.Transfer.Data
{
    /// <summary>
    /// Represents a setting mapping configuration
    /// </summary>
    public partial class PaymentTransferMap : NopEntityTypeConfiguration<PaymentTransfer>
    {
        #region Methods

        /// <summary>
        /// Configures the Bank
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PaymentTransfer> builder)
        {
            builder.ToTable(nameof(PaymentTransfer));
            builder.HasKey(transfer => transfer.Id);
            builder.Property(transfer => transfer.ReceiverBankId).IsRequired();
            builder.Property(transfer => transfer.IssuingBankId).IsRequired();
            builder.Property(transfer => transfer.OrderId).IsRequired();
            builder.Property(transfer => transfer.ReferenceNumber).HasMaxLength(50).IsRequired();

            builder.Property(transfer => transfer.ReceiverBankName).HasMaxLength(200).IsRequired();
            builder.Property(transfer => transfer.IssuingBankName).HasMaxLength(200).IsRequired();
            builder.Property(transfer => transfer.OrderTotal);
            builder.Property(transfer => transfer.PaymentStatusOrder).IsRequired();
            base.Configure(builder);
        }

        #endregion
    }
}