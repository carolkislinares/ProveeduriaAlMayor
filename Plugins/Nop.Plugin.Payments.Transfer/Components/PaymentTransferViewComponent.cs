using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.Transfer.Models;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Transfer.Components
{
    [ViewComponent(Name = "PaymentTransfer")]
    public class PaymentTransferViewComponent : NopViewComponent
    {

        private readonly IBankService _bankService;

        public PaymentTransferViewComponent(IBankService bankService)
        {
            _bankService = bankService;
        }

        public IViewComponentResult Invoke()
        {
            var allBank = _bankService.GetBankAlls();
            var listaBancos = new List<SelectListItem>();
            foreach (var bank in allBank)
            {
                listaBancos.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
            }
            var allBankReceiver = _bankService.GetBankReceiver();
            var listaBancosReceiver = new List<SelectListItem>();

            foreach (var bank in allBankReceiver)
            {
                listaBancosReceiver.Add(new SelectListItem { Text = bank.Name , Value = bank.AccountNumber.ToString() });
            }
            var model = new PaymentInfoModel
            {
                BancoEmisor = listaBancos,
                BancoReceptor = listaBancosReceiver
            };

            return View("~/Plugins/Payments.Transfer/Views/PaymentInfo.cshtml", model);
        }
    }
}
