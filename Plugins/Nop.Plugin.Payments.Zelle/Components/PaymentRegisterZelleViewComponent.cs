using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Zelle.Models;
using Nop.Plugin.Payments.Zelle.Services;
using Nop.Web.Framework.Components;
using System;

namespace Nop.Plugin.Payments.Zelle.Components
{
    [ViewComponent(Name = "PaymentRegisterZelle")]
    public class PaymentRegisterZelleViewComponent : NopViewComponent
    {
        private readonly IPaymentZelleService _zelleService;

        public PaymentRegisterZelleViewComponent(IPaymentZelleService zelleService)
        {
            
            this._zelleService = zelleService;
        }

        public IViewComponentResult Invoke(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new ArgumentNullException(nameof(id));
                }

                var zelle = _zelleService.GetPaymentZelleByOrderId(id);

                var model = new PaymentInfoModel();
                if (zelle != null)
                {
                    model = new PaymentInfoModel
                    {
                        IdZelle = zelle.Id,
                        Order = zelle.OrderId,
                        IssuingEmail = zelle.IssuingEmail,
                        Reference = zelle.ReferenceNumber, 
                        PaymentStatusOrder = zelle.PaymentStatusOrder
                    };
                }
                else
                {
                    model.Order = id;
                }
                return View("~/Plugins/Payments.Zelle/Views/RegisterPayment.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
            
        }
    }
}
