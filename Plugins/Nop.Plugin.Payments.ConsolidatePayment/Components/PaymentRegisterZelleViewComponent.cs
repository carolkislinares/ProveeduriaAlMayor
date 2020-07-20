using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.ConsolidatePayment.Models;
using Nop.Plugin.Payments.ConsolidatePayment.Service;
using Nop.Plugin.Payments.Zelle.Services;
using Nop.Web.Framework.Components;
using System;

namespace Nop.Plugin.Payments.Zelle.Components
{
    [ViewComponent(Name = "PaymentRegisterZelle")]
    public class PaymentRegisterZelleViewComponent : NopViewComponent
    {
        private readonly IConsolidatePaymentServices _consolidateService;

        public PaymentRegisterZelleViewComponent(IConsolidatePaymentServices zelleService)
        {
            
            this._consolidateService = zelleService;
        }

        public IViewComponentResult Invoke(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new ArgumentNullException(nameof(id));
                }

                var zelle = _consolidateService.GetPaymentByOrderId(id);

                var model = new ConfigurationModel();
                if (zelle != null)
                {
                    model = new ConfigurationModel
                    {
                        Id = zelle.Id,
                        OrdenId = zelle.OrdenId,
                        EmailEmisor = zelle.EmailEmisor,
                        Referencia = zelle.Referencia,
                        //StatusPaymentOrder = zelle.StatusPaymentOrder
                        StatusPaymentOrder = zelle.StatusPaymentOrder.ToString()
                    };
                }
                else
                {
                    model.OrdenId = id;
                }
                return View("~/Plugins/Payments.ConsolidatePayment/Views/RegisterPaymentZelle.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
            
        }
    }
}
