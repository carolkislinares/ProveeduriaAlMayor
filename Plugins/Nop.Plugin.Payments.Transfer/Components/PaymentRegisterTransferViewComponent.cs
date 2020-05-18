using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.Transfer.Domain;
using Nop.Plugin.Payments.Transfer.Models;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Transfer.Components
{
    [ViewComponent(Name = "PaymentRegisterTransfer")]
    public class PaymentRegisterTransferViewComponent : NopViewComponent
    {

        private readonly IPaymentTransferService _transferService;
        private readonly IBankService _bankService;

        public PaymentRegisterTransferViewComponent(IPaymentTransferService transferService, IBankService bankservice)
        {
            this._transferService = transferService;
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
                var transfer = _transferService.GetPaymentTransferByOrderId(id);

                var model = new PaymentInfoModel();
                if (transfer != null)
                {
                    // preparamos el modelo para el mostrar.
                    listaBancosReceiver.Clear();
                    listaBancos.Clear();
                    //var bank = allBankReceiver.Cast<Bank>().SingleOrDefault(i => i.Id.Equals(Convert.ToInt32(transfer.ReceiverBankId)));
                    //if (bank != null){
                    //    listaBancosReceiver.Add(new SelectListItem { Text = bank.Name +" - "+ bank.AccountNumber, Value = bank.Id.ToString()});
                    //}


                    //var bank2 = allBank.Cast<Bank>().SingleOrDefault(i => i.Id.Equals(Convert.ToInt32(transfer.IssuingBankId)));
                    //{
                    //    listaBancos.Add(new SelectListItem { Text = bank2.Name, Value = bank2.Id.ToString() });
                    //}

                    foreach (var bank in allBankReceiver)
                    {
                        listaBancosReceiver.Add(new SelectListItem { Text = bank.Name + " - " + bank.AccountNumber, Value = bank.Id.ToString() });
                    }

                    foreach (var bank in allBank)
                    {
                        listaBancos.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
                    }


                    model = new PaymentInfoModel
                    {
                        IdPayment = transfer.Id,
                        NroOrden = transfer.OrderId,
                        BancoEmisor = listaBancos,
                        BancoReceptor = listaBancosReceiver,
                        BancoEmisorId = transfer.IssuingBankId.ToString(), 
                        BancoReceptorId = transfer.ReceiverBankId.ToString(),
                        Referencia = transfer.ReferenceNumber,
                        StatusPayment = transfer.PaymentStatusOrder
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

                    model.BancoEmisor = listaBancos;
                    model.BancoReceptor = listaBancosReceiver;
                    model.NroOrden = id;
                }

                return View("~/Plugins/Payments.Transfer/Views/RegisterPayment.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
           
        }
    }
}
