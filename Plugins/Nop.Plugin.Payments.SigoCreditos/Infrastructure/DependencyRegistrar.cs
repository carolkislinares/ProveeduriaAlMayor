using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.SigoCreditos.Data;
using Nop.Plugin.Payments.SigoCreditos.Domain;
using Nop.Plugin.Payments.SigoCreditos.Services;
using Nop.Web.Framework.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Infrastructure
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
            builder.RegisterType<SigoCreditosPaypalService>().As<ISigoCreditosPaypalService>().InstancePerLifetimeScope();
          

            //data context
            builder.RegisterPluginDataContext<SigoCreditosPayPalObjectContext>("nop_object_context_SigoCreditosPayPal");
            //override required repository with our custom context
            builder.RegisterType<EfRepository<SigoCreditosPaypal>>().As<IRepository<SigoCreditosPaypal>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_SigoCreditosPayPal"))
                .InstancePerLifetimeScope();


        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}
