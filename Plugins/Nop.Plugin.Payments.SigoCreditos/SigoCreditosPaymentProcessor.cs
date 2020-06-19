using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.SigoCreditos.Data;
using Nop.Plugin.Payments.SigoCreditos.Models;
using Nop.Plugin.Payments.SigoCreditos.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.SigoCreditos
{
    /// <summary>
    /// SigoCreditos payment processor
    /// </summary>
    public class SigoCreditosPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly SigoCreditosPaymentSettings _SigoCreditosPaymentSettings;
        private readonly SigoCreditosPayPalObjectContext _contextSCpaypal;

        #endregion

        #region Ctor

        public SigoCreditosPaymentProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            SigoCreditosPaymentSettings SigoCreditosPaymentSettings,
            SigoCreditosPayPalObjectContext contextSCpaypal
            )
        {
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._SigoCreditosPaymentSettings = SigoCreditosPaymentSettings;
            this._contextSCpaypal = contextSCpaypal;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = true
            };
            switch (_SigoCreditosPaymentSettings.TransactMode)
            {
                case TransactMode.Pending:
                    result.NewPaymentStatus = PaymentStatus.Pending;
                    break;
                case TransactMode.Authorize:
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    break;
                case TransactMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    result.AddError("Not supported transaction type");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _SigoCreditosPaymentSettings.AdditionalFee, _SigoCreditosPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = true
            };
            switch (_SigoCreditosPaymentSettings.TransactMode)
            {
                case TransactMode.Pending:
                    result.NewPaymentStatus = PaymentStatus.Pending;
                    break;
                case TransactMode.Authorize:
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    break;
                case TransactMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    result.AddError("Not supported transaction type");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return new CancelRecurringPaymentResult();
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new SigoCreditosInfoValidator(_localizationService);
            var model = new SigoCreditosInfoModel
            {
                //CardholderName = form["CardholderName"],
                //CardNumber = form["CardNumber"],
                //CardCode = form["CardCode"],
                //ExpireMonth = form["ExpireMonth"],
                //ExpireYear = form["ExpireYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return warnings;
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest
            {
                CreditCardType = form["CreditCardType"],
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentSigoCreditos/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "SigoCreditosInfo";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new SigoCreditosPaymentSettings
            {
                TransactMode = TransactMode.Pending
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Instructions", "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.TransactMode", "After checkout mark payment as");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.TransactMode.Hint", "Specify transaction mode.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SigoCreditos.PaymentMethodDescription", "Pay by credit / debit card");

            // string de los formularios en web
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Modal.Title", "Información");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Modal.Closed", "Cerrar");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Modal.Warning", "Advertencia");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Modal.MessageError", "Un error ha ocurrido durante el proceso de búsqueda del cliente. ");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Modal.CustomerNotFound", "El cliente que ha buscado no fue encontrado. ");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditosPaypal.Transaccion.Exitosa", " Transacción exitosa");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditosPaypal.Transaccion.Fallida", "Transacción fallida");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditosPaypal.Comision.Message", " Costo adicional por pasarela de pago $");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditosPaypal.AmountTotal.Message", " Total a pagar $ ");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Button.CustomerSearch", "Buscar Cliente");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.AddBalanceModel.ReceiverDocumentValue", " Documento de identidad	");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.CostumerName", " Nombre");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.CostumerLastName", "Apellido");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.CostumerPhone", "Teléfono");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.AddBalanceModel.TransactionAmount", "Monto del abono");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.DocumentType", " Tipo de documento");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.CustomerDocumentValue", "Documento de identidad");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.OwnerBalanceFalse", "Terceros. ");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.OwnerBalanceTrue", "Cuenta propia. ");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.input.OwnerBalance", "Dirigido a: ");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.cardheader.titleCredit", "Abonar créditos");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.BalancPersonalDetails", " Balance de cuentas");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.AccountTypeName", "Tipo de cuenta");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.AccountTypeQuantity", "Monto");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.AccountTypeCurrency", "Moneda");
            _localizationService.AddOrUpdatePluginLocaleResource("abonoForm.CollapseButton.show", "Mostrar ");
            _localizationService.AddOrUpdatePluginLocaleResource("abonoForm.CollapseButton.hide", "Ocultar");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.SigoCreditos", "Sigo Créditos ");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.TransaccionPaypalID", "R. Paypal ");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Monto", "Monto");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.CedulaReceptor", "Cédula Receptor");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.NombreReceptor", "Receptor");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Estatus_Operacion", "Estatus");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditos.Account.TransactionDetails", "Abonos realizados");
             _localizationService.AddOrUpdatePluginLocaleResource("Account.EmptyTable", "No hay registros");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.ReferenceSigoCreditos", "R. SigoClub");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditosPaypal.Transaccion.FailAbono", "Pero existió un problema al abonar, contacte al administrador");
            _localizationService.AddOrUpdatePluginLocaleResource("SigoCreditosPaypal.CustomerCRM.NotFound", "Cliente no encontrado.");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.CreateDate", "Fecha");

            _contextSCpaypal.Install();



            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SigoCreditosPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Instructions");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.TransactMode");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.Fields.TransactMode.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SigoCreditos.PaymentMethodDescription");



            _localizationService.DeletePluginLocaleResource("SigoCreditos.Modal.Title");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.Modal.Closed");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.Modal.Warning");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.Modal.MessageError");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.Modal.CustomerNotFound");
            _localizationService.DeletePluginLocaleResource("SigoCreditosPaypal.Transaccion.Exitosa");
            _localizationService.DeletePluginLocaleResource("SigoCreditosPaypal.Transaccion.Fallida");
            _localizationService.DeletePluginLocaleResource("SigoCreditosPaypal.Comision.Message");
            _localizationService.DeletePluginLocaleResource("SigoCreditosPaypal.AmountTotal.Message");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.Button.CustomerSearch");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.AddBalanceModel.ReceiverDocumentValue");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.CostumerName");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.CostumerLastName");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.CostumerPhone");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.AddBalanceModel.TransactionAmount");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.DocumentType");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.CustomerDocumentValue");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.OwnerBalanceFalse ");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.OwnerBalanceTrue");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.input.OwnerBalance");
            _localizationService.DeletePluginLocaleResource("SigoCreditos.cardheader.titleCredit");
            _localizationService.DeletePluginLocaleResource("Account.BalancPersonalDetails");
            _localizationService.DeletePluginLocaleResource("Account.AccountTypeName");
            _localizationService.DeletePluginLocaleResource("Account.AccountTypeQuantity");
            _localizationService.DeletePluginLocaleResource("Account.AccountTypeCurrency");
            _localizationService.DeletePluginLocaleResource("abonoForm.CollapseButton.show");
            _localizationService.DeletePluginLocaleResource("abonoForm.CollapseButton.hide");
            _localizationService.DeletePluginLocaleResource("Account.SigoCreditos");
            _localizationService.DeletePluginLocaleResource("Account.TransaccionPaypalID");
            _localizationService.DeletePluginLocaleResource("Account.Monto");
            _localizationService.DeletePluginLocaleResource("Account.CedulaReceptor");
            _localizationService.DeletePluginLocaleResource("Account.NombreReceptor");
            _localizationService.DeletePluginLocaleResource("Account.Estatus_Operacion");
            _localizationService.DeletePluginLocaleResource("Account.ReferenceSigoCreditos");
            _localizationService.DeletePluginLocaleResource("SigoCreditosPaypal.Transaccion.FailAbono");
            _localizationService.DeletePluginLocaleResource("SigoCreditosPaypal.CustomerCRM.NotFound");
            _localizationService.DeletePluginLocaleResource("Account.CreateDate");


            _contextSCpaypal.Uninstall();
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.Manual; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Standard; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.SigoCreditos.PaymentMethodDescription"); }
        }

        #endregion

    }
}