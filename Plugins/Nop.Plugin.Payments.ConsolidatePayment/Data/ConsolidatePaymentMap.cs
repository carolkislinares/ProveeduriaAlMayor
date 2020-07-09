using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.ConsolidatePayment.Domain;


namespace Nop.Plugin.Payments.ConsolidatePayment.Data
{
   public partial class ConsolidatePaymentMap : NopEntityTypeConfiguration<Consolidate>
    {
        #region Methods

        /// <summary>
        /// Configures the ConsolidatePayment
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Consolidate> builder)
        {
            builder.ToTable(nameof(Consolidate));
            builder.HasKey(payment => payment.Id);
            base.Configure(builder);
        }

        #endregion
    }
}
