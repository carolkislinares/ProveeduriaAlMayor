using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Zelle;
using Nop.Plugin.Payments.Zelle.Domain;
using Nop.Plugin.Payments.Zelle.Models;
using Nop.Plugin.Payments.Zelle.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Zelle.Controllers
{

    [Area(AreaNames.Admin)]
    public class PaymentZelleController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPaymentZelleService _zelleService;

        #endregion

        #region Ctor

        public PaymentZelleController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPaymentZelleService zelleService)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._zelleService = zelleService;

        }

        #endregion

        #region Methods AuthorizeAdmin

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var zellePaymentSettings = _settingService.LoadSetting<ZellePaymentSettings>(storeScope);

            //var model = new ConfigurationModel
            //{
            //    TransactModeId = Convert.ToInt32(zellePaymentSettings.TransactMode),
            //    AdditionalFee = zellePaymentSettings.AdditionalFee,
            //    AdditionalFeePercentage = zellePaymentSettings.AdditionalFeePercentage,
            //    TransactModeValues = zellePaymentSettings.TransactMode.ToSelectList(),
            //    ActiveStoreScopeConfiguration = storeScope
            //};
            //if (storeScope > 0)
            //{
            //    model.TransactModeId_OverrideForStore = _settingService.SettingExists(zellePaymentSettings, x => x.TransactMode, storeScope);
            //    model.AdditionalFee_OverrideForStore = _settingService.SettingExists(zellePaymentSettings, x => x.AdditionalFee, storeScope);
            //    model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(zellePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            //}

            return View("~/Plugins/Payments.Zelle/Views/Configure.cshtml");
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var zellePaymentSettings = _settingService.LoadSetting<ZellePaymentSettings>(storeScope);

            //save settings
            //zellePaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            //zellePaymentSettings.AdditionalFee = model.AdditionalFee;
            //zellePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            //_settingService.SaveSettingOverridablePerStore(zellePaymentSettings, x => x.TransactMode, model.TransactModeId_OverrideForStore, storeScope, false);
            //_settingService.SaveSettingOverridablePerStore(zellePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            //_settingService.SaveSettingOverridablePerStore(zellePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }


        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult List(string referencia, DataSourceRequest command)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedKendoGridJson();

                var transferList = _zelleService.GetAllPaymentZelle(referencia, pageIndex: command.Page - 1, pageSize: command.PageSize);

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
        public IActionResult UpdateStateOrder(int orderid)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                    return AccessDeniedView();
                if (orderid == 0)
                {
                    return RedirectToAction("Configure");
                }

                _zelleService.UpdateStateOrderPaymentZelle(orderid);
                ViewBag.RefreshPage = true;
                return RedirectToAction("Configure");
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }


        #endregion

        #region  Methods
        public IActionResult RegisterPayment(int id)
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
                        Order = zelle.OrderId,
                        IssuingEmail = zelle.IssuingEmail,
                        Reference = zelle.ReferenceNumber
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

        [HttpPost]
        public IActionResult RegisterPayment(PaymentInfoModel payment)
        {
            try
            {

                if (payment == null)
                {
                    return View();
                }
                if (payment.IdZelle == 0)
                {
                    var model = new PaymentZelle
                    {
                        IssuingEmail = payment.IssuingEmail,
                        OrderId = payment.Order,
                        ReferenceNumber = payment.Reference
                    };

                    _zelleService.InsertPaymentZelle(model);
                }
                else
                {
                    var model = new PaymentZelle
                    {
                        Id = payment.IdZelle,
                        IssuingEmail = payment.IssuingEmail,
                        ReferenceNumber = payment.Reference
                    };

                    _zelleService.UpdatePaymentZelle(model);
                }

                ViewBag.RefreshPage = true;
                // return JavaScript("window.close();");
                return RedirectToRoute("OrderDetails", new { orderId = payment.Order });
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex);
            }
        }

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


    }
}