using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.Zelle.Data;
using Nop.Plugin.Payments.Zelle.Domain;
using Nop.Plugin.Payments.Zelle.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Payments.Zelle.Infrastructure
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
           
            builder.RegisterType<PaymentZelleService>().As<IPaymentZelleService>().InstancePerLifetimeScope();

            //           //data context
            builder.RegisterPluginDataContext<PaymentZelleObjectContext>("nop_object_context_payment_zelle");
            //override required repository with our custom context
            builder.RegisterType<EfRepository<PaymentZelle>>().As<IRepository<PaymentZelle>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_payment_zelle"))
                .InstancePerLifetimeScope();


        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}