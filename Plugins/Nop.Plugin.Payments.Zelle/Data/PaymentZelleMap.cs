using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.Zelle.Domain;

namespace Nop.Plugin.Payments.Zelle.Data
{
    /// <summary>
    /// Represents a setting mapping configuration
    /// </summary>
    public partial class PaymentZelleMap : NopEntityTypeConfiguration<PaymentZelle>
    {
        #region Methods

        /// <summary>
        /// Configures the Bank
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PaymentZelle> builder)
        {
            builder.ToTable(nameof(PaymentZelle));
            builder.HasKey(zelle => zelle.Id);
            builder.Property(zelle => zelle.IssuingEmail).HasMaxLength(50).IsRequired();
            builder.Property(zelle => zelle.OrderId).IsRequired();
            builder.Property(zelle => zelle.ReferenceNumber).HasMaxLength(50).IsRequired();
            builder.Property(zelle => zelle.OrderTotal);
            builder.Property(zelle => zelle.PaymentStatusOrder).IsRequired();
            base.Configure(builder);
        }

        #endregion
    }
}