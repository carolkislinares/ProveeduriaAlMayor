using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.Transfer.Domain;
using Nop.Plugin.Payments.Transfer.Models;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Transfer.Controllers
{
    
    [Area(AreaNames.Admin)]
    public class PaymentTransferController : BasePaymentController
    {
        #region Fields


        private readonly IPermissionService _permissionService;
        private readonly IPaymentTransferService _transferService;
        private readonly IBankService _bankService;
        #endregion

        #region Ctor

        public PaymentTransferController(IPermissionService permissionService, IPaymentTransferService transferService, IBankService bankservice)
        {
            this._permissionService = permissionService;
            this._transferService = transferService;
            this._bankService = bankservice;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                    return AccessDeniedView();

            
                var allBank = _bankService.GetBankAlls();
                var listaBancos = new List<SelectListItem>();
                listaBancos.Add(new SelectListItem { Text = "SELECCIONE", Value = 0.ToString() });
                foreach (var bank in allBank)
                {
                    listaBancos.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
                }

                var allBankReceiver = _bankService.GetBankReceiver();
                var listaBancosReceiver = new List<SelectListItem>();
                listaBancosReceiver.Add(new SelectListItem { Text = "SELECCIONE", Value = 0.ToString() });
                foreach (var bank in allBankReceiver)
                {
                    listaBancosReceiver.Add(new SelectListItem { Text = bank.Name, Value = bank.Id.ToString() });
                }

                var model = new PaymentTransferSearch();
                model.BancoEmisor = listaBancos;
                model.BancoReceptor = listaBancosReceiver;

                return View("~/Plugins/Payments.Transfer/Views/Configure.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }

        }

        [HttpPost]
        public IActionResult RegisterPayment(PaymentInfoModel payment)
        {
            try
            {
                if (payment == null)
                {
                    return Json(new { success = false });
                }
                if (payment.IdPayment == 0)
                {
                    var model = new PaymentTransfer
                    {
                        IssuingBankId = int.Parse(payment.BancoEmisorId),
                        OrderId = payment.NroOrden,
                        ReceiverBankId = int.Parse(payment.BancoReceptorId),
                        ReferenceNumber = payment.Referencia
                    };
                    _transferService.InsertPaymentTransfer(model);
                }
                else {

                    var model = new PaymentTransfer
                    {
                        Id = payment.IdPayment,
                        IssuingBankId = int.Parse(payment.BancoEmisorId),
                        OrderId = payment.NroOrden,
                        ReceiverBankId = int.Parse(payment.BancoReceptorId),
                        ReferenceNumber = payment.Referencia, 
                        PaymentStatusOrder = payment.StatusPayment
                    };
                    _transferService.UpdatePaymentTransfer(model);

                }


             
                ViewBag.RefreshPage = true;

                return RedirectToRoute("OrderDetails", new { orderId = payment.NroOrden });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        [HttpGet]
        public IActionResult BackOrder(int id)
        {
            try
            {
                return RedirectToRoute("OrderDetails", new { orderId = id });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        #endregion

        #region Methods AuthorizeAdmin

        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult UpdateStatePaymentTransfer(int orderid)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();
                if (orderid == 0)
                {
                    return RedirectToAction("Configure");
                }

                _transferService.UpdateStatePaymentTransfer(orderid);
                ViewBag.RefreshPage = true;
                return RedirectToAction("Configure");
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult List(DataSourceRequest command)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedKendoGridJson();

                var transferList = _transferService.GetAllPaymentTransfer(pageIndex: command.Page - 1, pageSize: command.PageSize);

                return Json(new DataSourceResult
                {
                    Data = transferList,
                    Total = transferList.TotalCount
                });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }



        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult Search(PaymentTransferSearch search, DataSourceRequest command)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedKendoGridJson();

                var transferList = _transferService.SearchTransfer(search, pageIndex: command.Page - 1, pageSize: command.PageSize);

                return Json(new DataSourceResult
                {
                    Data = transferList,
                    Total = transferList.TotalCount
                });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }
        #endregion

    }

}