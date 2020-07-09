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
using Nop.Services.Common;
using Nop.Services.Customers;
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
        private readonly IWorkContext _workContext;
        private readonly ISigoCreditosPaypalService _SigoCreditosPayPalService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public SigoCreditosInfoController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ISigoCreditosPaypalService SigoCreditosPayPalService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._SigoCreditosPayPalService = SigoCreditosPayPalService;
            this._genericAttributeService = genericAttributeService;
            this._customerService = customerService;
            this._workContext = workContext;


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



        /// <summary>
        /// Abona credito por abonos directos o Giftcard en CRM y guarda los datos de la transaccion de paypal. 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Abonar(SigoCreditosInfoModel model)
        {

            bool esGiftCard = string.IsNullOrWhiteSpace(model.Abono.IndGiftCard) ? false : Convert.ToBoolean(model.Abono.IndGiftCard);
            if (esGiftCard)
                model.Abono.MontoTransaccion = model.Abono.MontoTransaccionGiftCard;

            TransaccionModel transaccion = new TransaccionModel(model);
            try
            {
                if (!ModelState.IsValid)
                    return Configure();
                
                var result =esGiftCard  ? CRMContext.CRMContext.EnviarGiftCard(model) : CRMContext.CRMContext.AbonarPuntos(model);
                if (result != null)
                {
                    model.Abono.Cod_Abono = result.Cod_Abono;
                    transaccion.Cod_Abono = result.Cod_Abono;
                    model.Abono.TransaccionPayPalId = transaccion.TransaccionPaypalID;
                    transaccion.Estatus_Operacion = result.Cod_Abono != 0 ? true : false;
                    InsertarPaypal(transaccion);
                    return Json(model);
                }
                else
                {
                    transaccion.Estatus_Operacion = false;
                    model.Abono.TransaccionPayPalId = transaccion.TransaccionPaypalID;
                    InsertarPaypal(transaccion);
                    return Json(model);
                }
            }
            catch (Exception)
            {
                model.Abono.TransaccionPayPalId = transaccion.TransaccionPaypalID;
                transaccion.Estatus_Operacion = false;
                InsertarPaypal(transaccion);
                return Json(model);
                //throw ex;
            }
        }

        /// <summary>
        /// Guarda los datos de la transaccion de paypal
        /// </summary>
        /// <param name="model"></param>
        /// <param name="estatus"></param>
        private void InsertarPaypal(TransaccionModel model)
        {
            try
            {
                var SCPaypalmodel = new SigoCreditosPaypal
                {
                    TransaccionPaypalID = model.TransaccionPaypalID,
                    TransaccionCreditID = model.Cod_Abono,
                    CedulaReceptor = model.CedulaReceptor,
                    Estatus_Operacion = model.Estatus_Operacion,
                    Monto = model.Monto,
                    FechaCreacion = DateTime.Now,
                    NombreReceptor = model.NombreReceptor,
                    CustomerID = model.CustomerID,
                    EsGiftCard = model.IndGiftCard,
                    CodigoGiftCard = model.CodigoGiftCard
                };
                _SigoCreditosPayPalService.InsertSigoCreditosPaypal(SCPaypalmodel);
            }
            catch(Exception ex)
            {

                throw new NopException("Error al InsertarPaypal: " + ex.Message, ex);
            }
        }
        #region Pago Paypal Details

        /// <summary>
        /// Permite buscar la informacion de un cliente en CRM.
        /// </summary>
        /// <param name="tipoDocumento"></param>
        /// <param name="documento"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BuscarClienteSigo(int tipoDocumento, string documento)
        {
            try
            {
                ClienteModel result = CRMContext.CRMContext.ObtenerCliente(tipoDocumento, documento);
                if (result.EntityId == 0) { result = null; }
                return Json(result);
            }
            catch (Exception)
            {
                return Json(null);
            } 
        }

        /// <summary>
        /// Metodo para validar el pin del usuario
        /// </summary>
        /// <param name="entityid"> entityid(si tipoDoc==1) o sigocludid(si tipoDoc==2) del cliente logueado</param>
        /// <param name="cedula"> cedula del cliente a consultar</param>
        /// <param name="pin">pin del cliente a consultar</param>
        /// <param name="tipoDoc">tipo de cliente logueado 1- natural 2- Juridico</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ValidarPin(long entityid, string cedula,string pin, string tipoDoc)
        {
            try
            {
                if (tipoDoc.Contains("2"))
                {
                    entityid = CRMContext.ApiCloudContext.ValidarAutorizado(entityid, cedula);
                    if(entityid==0)
                        return Json(2);
                }
            
                 bool result = CRMContext.ApiCloudContext.ConfirmarPassword(entityid, cedula, pin);
                 _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, "EsValidoCRM", result);
                
                 return Json(Convert.ToInt16(result));
            }
            catch(Exception)
            {
                return Json(null);
            }
        }


        //[HttpPost]
        //[AdminAntiForgery]
        //public IActionResult AbonarGiftCard(SigoCreditosGiftCardModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
        //        return AccessDeniedView();

        //    if (!ModelState.IsValid)
        //        return Configure();

        //    //load settings for a chosen store scope

        //    bool result = CRMContext.CRMContext.EnviarGiftCard(model);
        //    if (result)
        //    {
        //        var cliente = CRMContext.CRMContext.ObtenerPuntosxCliente(model.DocumentType, model.DocumentValue);
        //        return RedirectToRoute("CustomerSigoCreditos");
        //        //return View("~/Plugins/Payments.SigoCreditos/Views/SigoCreditosInfo.cshtml", cliente);
        //        // return View("~/Plugins/Payments.SigoCreditos/Views/SigoCreditosInfo.cshtml", null);
        //    }
        //    else
        //    {
        //        return Configure();
        //    }

        //    //now clear settings cache

        //}

        #endregion
        #endregion
    }
}