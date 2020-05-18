using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Configuration;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.Transfer.Domain;

namespace Nop.Plugin.Payments.Transfer.Data
{
    /// <summary>
    /// Represents a setting mapping configuration
    /// </summary>
    public partial class BankMap : NopEntityTypeConfiguration<Bank>
    {
        #region Methods

        /// <summary>
        /// Configures the Bank
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.ToTable(nameof(Bank));
            builder.HasKey(bank => bank.Id);
            builder.Property(bank => bank.Name).HasMaxLength(200).IsRequired();
            builder.Property(bank => bank.IsReceiver).HasMaxLength(1).IsRequired();
            builder.Property(bank => bank.AccountNumber).HasMaxLength(50);
            base.Configure(builder);
        }

        #endregion
    }
}