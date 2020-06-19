using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.SigoCreditos.Domain;
using Nop.Plugin.Payments.SigoCreditos.Models;
using Nop.Plugin.Payments.SigoCreditos.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.SigoCreditos.Controllers
{
   
    [Area(AreaNames.Admin)]
    public class SigoCreditosInfoController : BasePaymentController
    {
        #region Fields
        
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ISigoCreditosPaypalService _SigoCreditosPayPalService;

        #endregion

        #region Ctor

        public SigoCreditosInfoController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ISigoCreditosPaypalService SigoCreditosPayPalService)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._SigoCreditosPayPalService = SigoCreditosPayPalService;
        }

        #endregion

        #region Methods



        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var SigoCreditosPaymentSettings = _settingService.LoadSetting<SigoCreditosPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                TransactModeId = Convert.ToInt32(SigoCreditosPaymentSettings.TransactMode),
                AdditionalFee = SigoCreditosPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = SigoCreditosPaymentSettings.AdditionalFeePercentage,
                TransactModeValues = SigoCreditosPaymentSettings.TransactMode.ToSelectList(),
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.TransactModeId_OverrideForStore = _settingService.SettingExists(SigoCreditosPaymentSettings, x => x.TransactMode, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(SigoCreditosPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(SigoCreditosPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.SigoCreditos/Views/Configure.cshtml", model);
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
            var SigoCreditosPaymentSettings = _settingService.LoadSetting<SigoCreditosPaymentSettings>(storeScope);

            //save settings
            SigoCreditosPaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            SigoCreditosPaymentSettings.AdditionalFee = model.AdditionalFee;
            SigoCreditosPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            _settingService.SaveSettingOverridablePerStore(SigoCreditosPaymentSettings, x => x.TransactMode, model.TransactModeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(SigoCreditosPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(SigoCreditosPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }





        [HttpPost]
        public IActionResult Abonar(SigoCreditosInfoModel model)
        {
            long Cod_Abono=0;
            try
            {
                if (!ModelState.IsValid)
                    return Configure();
                var result = CRMContext.CRMContext.AbonarPuntos(model);
                if (result != null)
                {

                    var SCPaypalmodel = new SigoCreditosPaypal
                    {

                        TransaccionPaypalID = model.IdTransaccion,
                        TransaccionCreditID = result.Cod_Abono,
                        CedulaReceptor = model.AddBalanceModel.OwnerBalance == 1 ? model.CustomerDocumentValue : model.AddBalanceModel.ReceiverDocumentValue,
                        Estatus_Operacion = result.Cod_Abono != 0 ? true : false,
                        Monto = Convert.ToDecimal(model.AddBalanceModel.TransactionAmount),
                        FechaCreacion = DateTime.Now,
                        NombreReceptor = model.CostumerName,
                        CustomerID = model.CustomerID,
                        
                    };
                    model.Cod_Abono = result.Cod_Abono;
                    _SigoCreditosPayPalService.InsertSigoCreditosPaypal(SCPaypalmodel);
                    return Json(model);
                    //return RedirectToRoute("CustomerSigoCreditos");
                }
                else
                {
                    return Configure();
                }

            }
            catch (Exception ex)
            {
                var SCPaypalmodel = new SigoCreditosPaypal
                {

                    TransaccionPaypalID = model.IdTransaccion,
                    TransaccionCreditID = model.Cod_Abono,
                    CedulaReceptor = model.AddBalanceModel.OwnerBalance == 1 ? model.CustomerDocumentValue : model.AddBalanceModel.ReceiverDocumentValue,
                    Estatus_Operacion = false,
                    Monto = Convert.ToDecimal(model.AddBalanceModel.TransactionAmount.Replace(".", string.Empty).Replace(",", ".").Trim()),
                    FechaCreacion = DateTime.Now,
                    NombreReceptor = model.CostumerName,
                    CustomerID = model.CustomerID,

                };
                _SigoCreditosPayPalService.InsertSigoCreditosPaypal(SCPaypalmodel);
                return Json(model);
                //throw ex;
            }


            //now clear settings cache

        }




        #region Pago Paypal Details


        [HttpPost]

        public JsonResult BuscarClienteSigo(int tipoDocumento, string documento)
        {

            //load settings for a chosen store scope
            // return RedirectToRoute("CustomerSigoCreditos");
            SigoCreditosInfoModel result = CRMContext.CRMContext.ObtenerCliente(tipoDocumento, documento);

            //SigoCreditosInfoModel result = new SigoCreditosInfoModel
            //{
            //    CostumerLastName = "Linares",
            //    SigoClubId = Convert.ToInt64(new Random().Next(1000, 2000)),
            //    EntityId = Convert.ToInt64(new Random().Next(1000, 2000)),
            //    CostumerName = "Carol",
            //    CostumerPhone = "0295-65874023",
            //    CustomerDocumentType = tipoDocumento,
            //    CustomerDocumentValue = documento
            //};
            if (result.EntityId == 0) { result = null; }


            return Json(result);


            //now clear settings cache

        }




        [HttpPost]
        [AdminAntiForgery]
        public IActionResult AbonarGiftCard(SigoCreditosGiftCardModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope

            bool result = CRMContext.CRMContext.EnviarGiftCard(model);
            if (result)
            {
                var cliente = CRMContext.CRMContext.ObtenerPuntosxCliente(model.DocumentType, model.DocumentValue);
                return RedirectToRoute("CustomerSigoCreditos");
                //return View("~/Plugins/Payments.SigoCreditos/Views/SigoCreditosInfo.cshtml", cliente);
                // return View("~/Plugins/Payments.SigoCreditos/Views/SigoCreditosInfo.cshtml", null);
            }
            else
            {
                return Configure();
            }

            //now clear settings cache

        }

        #endregion
        #endregion
    }
}