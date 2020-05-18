using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.Transfer.Data;
using Nop.Plugin.Payments.Transfer.Domain;
using Nop.Plugin.Payments.Transfer.Models;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Payments.Transfer.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<BankService>().As<IBankService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentTransferService>().As<IPaymentTransferService>().InstancePerLifetimeScope();


            //data context
            builder.RegisterPluginDataContext<BankObjectContext>("nop_object_context_bank");
            //override required repository with our custom context
            builder.RegisterType<EfRepository<Bank>>().As<IRepository<Bank>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bank"))
                .InstancePerLifetimeScope();

            //           //data context
            builder.RegisterPluginDataContext<PaymentTransferObjectContext>("nop_object_context_payment_transfer");
            //override required repository with our custom context
            builder.RegisterType<EfRepository<PaymentTransfer>>().As<IRepository<PaymentTransfer>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_payment_transfer"))
                .InstancePerLifetimeScope();

            //data context
            builder.RegisterPluginDataContext<PaymentTransferObjectContext>("nop_object_context_payment_transfer");
            //override required repository with our custom context
            builder.RegisterType<EfRepository<PaymentTransfer>>().As<IRepository<PaymentTransfer>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_payment_transfer"))
                .InstancePerLifetimeScope();


           // builder.RegisterPluginDataContext<PaymentTransferListObjectContext>("nop_object_context_payment_transfer_list");
            ////override required repository with our custom context
            builder.RegisterType<PaymentTransferSearch>().As<PaymentTransferSearch>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_payment_transfer_list"))
                .InstancePerLifetimeScope();


            
          
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}