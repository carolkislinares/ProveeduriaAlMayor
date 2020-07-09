using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.ConsolidatePayment.Data;
using Nop.Plugin.Payments.ConsolidatePayment.Domain;
using Nop.Plugin.Payments.ConsolidatePayment.Service;
using Nop.Web.Framework.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.ConsolidatePayment.Infraestructure
{
    class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ConsolidatePaymentServices>().As<IConsolidatePaymentServices>().InstancePerLifetimeScope();
            //data context
            builder.RegisterPluginDataContext<ConsolidatePaymentObjectContext>("nop_object_context_ConsolidatePayment");
            //override required repository with our custom context
            builder.RegisterType<EfRepository<Consolidate>>().As<IRepository<Consolidate>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_ConsolidatePayment"))
                .InstancePerLifetimeScope();


        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}
