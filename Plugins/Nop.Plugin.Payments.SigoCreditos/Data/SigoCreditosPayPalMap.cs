using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.SigoCreditos.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Data
{
    public partial class SigoCreditosPayPalMap : NopEntityTypeConfiguration<SigoCreditosPaypal>
    {
        #region Methods
        /// <summary>
        /// Configures the SigoCreditosPayPal
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        /// 
        public override void Configure(EntityTypeBuilder<SigoCreditosPaypal> builder)
        {
            builder.ToTable(nameof(SigoCreditosPaypal));
            builder.HasKey(scPaypal => scPaypal.Id);
            builder.Property(scPaypal => scPaypal.NombreReceptor).HasMaxLength(200);
            builder.Property(scPaypal => scPaypal.TransaccionPaypalID).HasMaxLength(20).IsRequired();
            builder.Property(scPaypal => scPaypal.TransaccionCreditID).HasMaxLength(2).IsRequired();
            builder.Property(scPaypal => scPaypal.Monto).HasMaxLength(10);
            base.Configure(builder);
        }


        #endregion
    }
}
