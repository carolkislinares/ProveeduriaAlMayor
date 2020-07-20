using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.ConsolidatePayment.Models;
using Nop.Plugin.Payments.ConsolidatePayment.Service;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.ConsolidatePayment.Components
{
    [ViewComponent(Name = "PaymentConsolidatePayment")]
    public class PaymentConsolidatePaymentViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IBankService _bankService;
        private readonly IStoreService _storeService;
        public PaymentConsolidatePaymentViewComponent(IStoreContext storeContext, IBankService bankservice, IStoreService storeService)
        {
            this._storeContext = storeContext;
            this._bankService = bankservice;
            this._storeService = storeService;
        }


        public IViewComponentResult Invoke()
        {
           
            var Configuracion = new ConfigurationModel();
 			
			Configuracion.StatusPaymentOrderList = new List<SelectListItem> {
                new SelectListItem { Text = "SELECCIONE", Value = string.Empty },
                new SelectListItem { Text = "Pendiente", Value = "10" },
                new SelectListItem { Text = "Consolidado", Value = "30" },
             };
            Configuracion.MetodosPago = new List<SelectListItem>
            {
                new SelectListItem { Text = "SELECCIONE", Value = string.Empty },
                new SelectListItem { Text = "Transferencia Bancaria", Value = "Payments.Transfer" },
                new SelectListItem { Text = "Zelle", Value = "Payments.Zelle" },
             };

            Configuracion.Tiendas = new List<SelectListItem>
            {
               new SelectListItem { Text = _storeContext.CurrentStore.Name, Value = _storeContext.CurrentStore.Id.ToString() },
            };

            foreach (var tienda in _storeService.GetAllStores().Where(x => x.Id != _storeContext.CurrentStore.Id))
            {
                Configuracion.Tiendas.Add(new SelectListItem { Text = tienda.Name, Value = tienda.Id.ToString() });
            }

            var allBank = _bankService.GetBankAlls();
            var listaBancos = new List<SelectListItem>();
            listaBancos.Add(new SelectListItem { Text = "SELECCIONE", Value = 0.ToString() });
            foreach (var bank in allBank)
            {
                listaBancos.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
            }

            Configuracion.BancosReceptores = listaBancos;


            //load settings for a chosen store scope
            return View("~/Plugins/Payments.ConsolidatePayment/Views/Configure.cshtml", Configuracion);
        }
    }
}
