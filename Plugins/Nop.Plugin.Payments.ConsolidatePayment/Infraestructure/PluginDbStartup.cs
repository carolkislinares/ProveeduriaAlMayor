using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.ConsolidatePayment.Data;
using Nop.Web.Framework.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.ConsolidatePayment.Infraestructure
{
    class PluginDbStartup : INopStartup
    {

        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add object context
            services.AddDbContext<ConsolidatePaymentObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });
        }
        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
           
        }


        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 12;
    }

}
