﻿using System;
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
                new SelectListItem { Text = "SELECCIONE", Value = 0.ToString() },
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
            return View("~/Plugins/Payments.ConsolidatePayment/Views/Configure.cshtml", new ConfigurationModel());
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

            if (search != null)
                throw new ArgumentNullException(nameof(search));


            Consolidate payment = new Consolidate
            {
                TiendaId = search.OrdenId,
                OrdenId = search.OrdenId,
                ClienteId = search.ClienteId,
                Referencia = search.Referencia,
                MetodoPago = search.MetodoPago,
                StatusPaymentOrder = search.StatusPaymentOrder,
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

        //public virtual IActionResult ExportXml()
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();

        //    try
        //    {
        //        var xml = _exportManager.ExportCategoriesToXml();

        //        return File(Encoding.UTF8.GetBytes(xml), "application/xml", "categories.xml");
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("List");
        //    }
        //}

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







        //public virtual byte[] ExportPaymentToXlsx()
        //{
        //    var parentCatagories = new List<ConfigurationModel>();
        //    //if (_catalogSettings.ExportImportCategoriesUsingCategoryName)
        //    //{
        //    //    //performance optimization, load all parent categories in one SQL request

        //    //   var parent = _settingService.LoadSetting<ConsolidatePaymentPaymentSettings>(_storeContext.ActiveStoreScopeConfiguration);

        //    //    parentCatagories.TransactModeId = Convert.ToInt32(parent.TransactMode);
        //    //    parentCatagories.AdditionalFee = parent.AdditionalFee;
        //    //    parentCatagories.AdditionalFeePercentage = parent.AdditionalFeePercentage;
        //    //    parentCatagories.TransactModeValues = parent.TransactMode.ToSelectList();
        //    //    parentCatagories.ActiveStoreScopeConfiguration = _storeContext.ActiveStoreScopeConfiguration;


        //    //}

        //    //          //property manager 
        //    //var manager = new PropertyManager<ConfigurationModel>(new[]
        //    //{
        //    //    new PropertyByName<ConfigurationModel>("TransactModeId", p => p.TransactModeId),
        //    //    new PropertyByName<ConfigurationModel>("AdditionalFee", p => p.AdditionalFee),
        //    //    new PropertyByName<ConfigurationModel>("ActiveStoreScopeConfiguration", p => p.ActiveStoreScopeConfiguration),
        //    //    new PropertyByName<ConfigurationModel>("AdditionalFee_OverrideForStore", p => p.AdditionalFee_OverrideForStore)
        //    //}, _catalogSettings);


        //    //var configuration = new List<ConfigurationModel>();
        //    //configuration.Add(parentCatagories);

        //    //return manager.ExportToXlsx(configuration);
        //}



        #endregion
    }
}