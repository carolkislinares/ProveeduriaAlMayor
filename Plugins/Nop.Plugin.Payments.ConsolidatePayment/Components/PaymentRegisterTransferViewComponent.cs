using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.ConsolidatePayment.Models;
using Nop.Plugin.Payments.ConsolidatePayment.Service;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.ConsolidatePayment.Components
{
    [ViewComponent(Name = "PaymentRegisterTransfer")]
    public class PaymentRegisterTransferViewComponent : NopViewComponent
    {

        private readonly IConsolidatePaymentServices  _consolidateService;
        private readonly IBankService _bankService;

        public PaymentRegisterTransferViewComponent(IConsolidatePaymentServices paymentService, IBankService bankservice)
        {
            this._consolidateService = paymentService;
            this._bankService = bankservice;
        }

        public IViewComponentResult Invoke(int id)
        {
            try
            {
                var allBank = _bankService.GetBankAlls();
                var listaBancos = new List<SelectListItem>();
                var allBankReceiver = _bankService.GetBankReceiver();
                var listaBancosReceiver = new List<SelectListItem>();


                // verificamos que la orden no tenga transferencia registrada
                var transfer = _consolidateService.GetPaymentByOrderId(id);

                var model = new ConfigurationModel();
                if (transfer != null)
                {
                    // preparamos el modelo para el mostrar.
                    listaBancosReceiver.Clear();
                    listaBancos.Clear();
        
                    foreach (var bank in allBankReceiver)
                    {
                        listaBancosReceiver.Add(new SelectListItem { Text = bank.Name + " - " + bank.AccountNumber, Value = bank.Id.ToString() });
                    }

                    foreach (var bank in allBank)
                    {
                        listaBancos.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
                    }


                    model = new ConfigurationModel
                    {
                        Id = transfer.Id,
                        OrdenId = transfer.OrdenId,
                        BancosEmisores = listaBancos,
                        BancosReceptores = listaBancosReceiver,
                        BancoEmisorId = transfer.BancoEmisorId, 
                        BancoReceptorId = transfer.BancoReceptorId,
                        Referencia = transfer.Referencia,
                        //StatusPaymentOrder = transfer.StatusPaymentOrder
                        StatusPaymentOrder = transfer.StatusPaymentOrder.ToString()
                        
                    };
                }
                else
                {
                    // Creamos el modelo para el registrar.
                    listaBancosReceiver.Clear();
                    listaBancos.Clear();

                    foreach (var bank in allBankReceiver)
                    {
                        listaBancosReceiver.Add(new SelectListItem { Text = bank.Name + " - " + bank.AccountNumber, Value = bank.Id.ToString() });
                    }

                    foreach (var bank in allBank)
                    {
                        listaBancos.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
                    }

                    model.BancosEmisores = listaBancos;
                    model.BancosReceptores = listaBancosReceiver;
                    model.OrdenId = id;
                }

                return View("~/Plugins/Payments.ConsolidatePayment/Views/RegisterPaymentTransfer.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
           
        }
    }
}
