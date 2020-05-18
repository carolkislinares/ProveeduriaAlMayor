using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Zelle.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Zelle.Components
{
    [ViewComponent(Name = "PaymentZelle")]
    public class PaymentZelleViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.Zelle/Views/PaymentInfo.cshtml", model);
        }
    }
}
