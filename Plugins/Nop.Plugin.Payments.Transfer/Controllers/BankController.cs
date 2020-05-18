using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payments.Transfer.Domain;
using Nop.Plugin.Payments.Transfer.Models;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Transfer.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    public class BankController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IBankService _bankService;

        #endregion

        #region Ctor

        public BankController(ILocalizationService localizationService,
               IPermissionService permissionService,
               ISettingService settingService,
               IStoreContext storeContext,
               IBankService bankService)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._bankService = bankService;
        }

        #endregion

        #region Methods


        [HttpPost]
        [AdminAntiForgery]
        public IActionResult List(string name, DataSourceRequest command)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedKendoGridJson();

                var bankslist = _bankService.GetAllBanks(name, pageIndex: command.Page - 1, pageSize: command.PageSize);
                var model = bankslist.Select(point =>
                {
                    var bank = _bankService.GetBankById(point.StoreId);
                    return new BankModel
                    {
                        Id = bank.Id,
                        Name = bank.Name,
                        AccountNumber = bank.AccountNumber,
                        IsReceiver = bank.IsReceiver

                        //StoreId = store?.Name ?? (point.StoreId == 0 ? _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores") : string.Empty)
                    };
                }).ToList();

                return Json(new DataSourceResult
                {
                    Data = bankslist,
                    Total = bankslist.TotalCount
                });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        public IActionResult Create()
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();
                var model = new BankModel();
                return View("~/Plugins/Payments.Transfer/Views/Create.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Create(BankModel model)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();
                if(!model.IsReceiver && !string.IsNullOrWhiteSpace(model.AccountNumber))
                {
                    model.AccountNumber = string.Empty;
                }
                var bankModel = new Bank
                {
                    Name = model.Name,
                    AccountNumber = model.AccountNumber,
                    IsReceiver = model.IsReceiver,
                    StoreId = 1
                };
                _bankService.InsertBank(bankModel);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Payments.Transfer/Views/Create.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();

                var bank = _bankService.GetBankById(id);
                if (bank == null)
                    return RedirectToAction("Configure");

                var model = new BankModel
                {
                    Id = bank.Id,
                    Name = bank.Name,
                    IsReceiver = bank.IsReceiver,
                    AccountNumber = bank.AccountNumber,
                    StoreId = bank.StoreId
                };
                return View("~/Plugins/Payments.Transfer/Views/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Edit(BankModel model)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();

                var bank = _bankService.GetBankById(model.Id);
                if (bank == null)
                    return RedirectToAction("Configure");

                bank.Name = model.Name;
                bank.IsReceiver = model.IsReceiver;
                bank.AccountNumber = model.AccountNumber;

                _bankService.UpdateBank(bank);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Payments.Transfer/Views/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Delete(int id)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();

                var bank = _bankService.GetBankById(id);
                if (bank == null)
                    return RedirectToAction("Configure");
                _bankService.DeleteBank(bank);

                return new NullJsonResult();
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

        #endregion
    }
}
