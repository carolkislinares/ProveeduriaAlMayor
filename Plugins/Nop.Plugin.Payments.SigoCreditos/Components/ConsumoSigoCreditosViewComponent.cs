using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Payments.SigoCreditos.Components {
    [ViewComponent(Name = "ConsumoSigoCreditos")]
    public class ConsumoSigoCreditosViewComponent : NopViewComponent {

        public ConsumoSigoCreditosViewComponent() {
        }

        public IViewComponentResult Invoke() {

            
            return View("~/Plugins/Payments.SigoCreditos/Views/_ConsumoSigoCreditos.cshtml");
        }
    }
}
