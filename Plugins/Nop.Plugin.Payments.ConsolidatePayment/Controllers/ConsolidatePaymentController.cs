using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Payments.ConsolidatePayment.Domain;
using Nop.Plugin.Payments.ConsolidatePayment.Models;
using Nop.Plugin.Payments.ConsolidatePayment.Service;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.ConsolidatePayment.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PaymentConsolidatePaymentController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IConsolidatePaymentServices _consolidateService;
        private readonly IBankService _bankService;

        #endregion

        #region Ctor

        public PaymentConsolidatePaymentController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            CatalogSettings catalogSettings,
            IConsolidatePaymentServices consolidateService,
            IStoreService storeService,
            IBankService bankservice)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._catalogSettings = catalogSettings;
            this._consolidateService = consolidateService;
            this._storeService = storeService;
            this._bankService = bankservice;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();
            var Configuracion = new ConfigurationModel();

            Configuracion.MetodosPago = new List<SelectListItem>
            {
                new SelectListItem { Text = "TODOS", Value = string.Empty },
                new SelectListItem { Text = "Transferencia Bancaria", Value = "Payments.Transfer" },
                new SelectListItem { Text = "Zelle", Value = "Payments.Zelle" },
             };

            Configuracion.StatusPaymentOrderList = new List<SelectListItem> {
                new SelectListItem { Text = "TODOS", Value = string.Empty },
                new SelectListItem { Text = "Pendiente", Value = "10" },
                new SelectListItem { Text = "Consolidado", Value = "30" },
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


    [HttpPost]
    [AuthorizeAdmin]
    [AdminAntiForgery]
    public IActionResult List(ConfigurationModel search, DataSourceRequest command)
    {
        try
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedKendoGridJson();

            if (search == null)
                throw new ArgumentNullException(nameof(search));


            Consolidate payment = new Consolidate
            {
                TiendaId = search.TiendaId,
                OrdenId = search.OrdenId,
                ClienteId = search.ClienteId,
                Referencia = search.Referencia == null ? "" : search.Referencia,
                MetodoPago = search.MetodoPago == null ? "" : search.MetodoPago,
                //StatusPaymentOrder = search.StatusPaymentOrder,
                StatusPaymentOrder = Convert.ToInt32(search.StatusPaymentOrder),
                BancoEmisorId = search.BancoReceptorId,
                BancoReceptorId = search.BancoReceptorId
            };

            var transferList = _consolidateService.SearchPayment(payment, pageIndex: command.Page - 1, pageSize: command.PageSize);

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
        public IActionResult RegisterPayment(ConfigurationModel payment)
        {
            try
            {
                if (payment == null)
                {
                    return Json(new { success = false });
                }

                var model = new Consolidate
                {
                    BancoEmisorId = payment.BancoEmisorId,
                    BancoReceptorId = payment.BancoReceptorId,
                    OrdenId = payment.OrdenId,
                    BancoEmisor = payment.BancoEmisor,
                    BancoReceptor = payment.BancoReceptor,
                    EmailEmisor = payment.EmailEmisor,
                    FechaRegistro = DateTime.Now,
                    FechaUltimaActualizacion = DateTime.Now,
                    Referencia = payment.Referencia,
                    StatusPaymentOrder = Convert.ToInt32(payment.StatusPaymentOrder),
                    Tienda = _storeContext.CurrentStore.Name, 
                    Id = payment.Id

                };

                _consolidateService.InsertPayment(model);

                ViewBag.RefreshPage = true;

                return RedirectToRoute("OrderDetails", new { orderId = payment.OrdenId });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }


        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult UpdateStatePayment(int id)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();
                if (id == 0)
                    return RedirectToAction("Configure");
            
                var payment = _consolidateService.GetPaymentById(id);
                if (payment==null)
                    return RedirectToAction("Configure");

                _consolidateService.UpdateStatePayment(id);
                ViewBag.RefreshPage = true;
                return RedirectToAction("Configure");
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }
        #endregion

        #region Export / Import

        public virtual IActionResult ExportXlsxPendiente()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {

                Consolidate payment = new Consolidate
                {
                   
                    StatusPaymentOrder =10,
                   
                };

                var transferList = _consolidateService.SearchPayment(payment);

                var xml = ExportPaymentToXlsx(transferList);

                return File(xml, MimeTypes.TextXlsx, "PagosPendientes.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public virtual IActionResult ExportXlsxConsolidado()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {

                Consolidate payment = new Consolidate
                {

                    StatusPaymentOrder = 30,

                };

                var transferList = _consolidateService.SearchPayment(payment);

                var xml = ExportPaymentToXlsx(transferList);

                return File(xml, MimeTypes.TextXlsx, "PagosConsolidados.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public virtual IActionResult ExportXlsx(string status)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {

                Consolidate payment = new Consolidate
                {

                    StatusPaymentOrder = Convert.ToInt32(status),

                };

                var transferList = _consolidateService.SearchPayment(payment);

                var xml = ExportPaymentToXlsx(transferList);

                return File(xml, MimeTypes.TextXlsx, "Pagos.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        //public virtual IActionResult ExportXlsx()
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();

        //    try
        //    {
        //        var storeScope = _storeContext.ActiveStoreScopeConfiguration;
        //        var bytes = ExportPaymentToXlsx();

        //        return File(bytes, MimeTypes.TextXlsx, "Configuration.xlsx");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("Configure");
        //    }
        //}

        //  [HttpPost]d
        //public virtual IActionResult ImportFromXlsx(IFormFile importexcelfile)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();


        //    try
        //    {
        //        if (importexcelfile != null && importexcelfile.Length > 0)
        //        {
        //            _importManager.ImportCategoriesFromXlsx(importexcelfile.OpenReadStream());
        //        }
        //        else
        //        {
        //            ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
        //            return RedirectToAction("List");
        //        }

        //        SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Imported"));

        //        return RedirectToAction("List");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("List");
        //    }
        //}








        /// <summary>
        /// Export payment to XLSX
        /// </summary>
        /// <param name="payment">Categories</param>
        public virtual byte[] ExportPaymentToXlsx(IList<Consolidate> payment)
        {
          //  var parentCatagories = new List<Consolidate>();
          
            //property manager 
            var manager = new PropertyManager<Consolidate>(new[]
            {
                new PropertyByName<Consolidate>("Id", p => p.Id),
                new PropertyByName<Consolidate>("Fecha Última Actualización", p => p.FechaUltimaActualizacion.ToString("dd/MM/yyyy hh:mm:ss")),
                new PropertyByName<Consolidate>("Tienda", p => p.Tienda),
                new PropertyByName<Consolidate>("Metodo Pago", p => p.MetodoPago.Contains("Zelle")? "Zelle": "Transferencia Bancaria"),
                new PropertyByName<Consolidate>("Orden #", p => p.OrdenId),
                new PropertyByName<Consolidate>("Referencia", p => p.Referencia),
                new PropertyByName<Consolidate>("Banco/Email", p => string.IsNullOrEmpty(p.BancoReceptor)? p.EmailEmisor:p.BancoReceptor),
                new PropertyByName<Consolidate>("Monto Total", p => p.CodigoMoneda.Contains("Bs")? p.CodigoMoneda+p.MontoTotalOrden:"$"+p.MontoTotalOrden),
            }, _catalogSettings);

            return manager.ExportToXlsx(payment);
        }




        #endregion
    }
}