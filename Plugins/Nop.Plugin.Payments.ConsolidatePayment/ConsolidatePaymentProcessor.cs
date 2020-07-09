using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.ConsolidatePayment;
using Nop.Plugin.Payments.ConsolidatePayment.Data;
using Nop.Plugin.Payments.ConsolidatePayment.Models;
using Nop.Plugin.Payments.ConsolidatePayment.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.ConsolidatePayment
{
    /// <summary>
    /// ConsolidatePayment payment processor
    /// </summary>
    public class ConsolidatePaymentPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ConsolidatePaymentPaymentSettings _ConsolidatePaymentPaymentSettings;
        private readonly ConsolidatePaymentObjectContext _context;
        #endregion

        #region Ctor

        public ConsolidatePaymentPaymentProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            ConsolidatePaymentPaymentSettings ConsolidatePaymentPaymentSettings, 
            ConsolidatePaymentObjectContext context)
        {
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._ConsolidatePaymentPaymentSettings = ConsolidatePaymentPaymentSettings;
            this._context = context;
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
            switch (_ConsolidatePaymentPaymentSettings.TransactMode)
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
                _ConsolidatePaymentPaymentSettings.AdditionalFee, _ConsolidatePaymentPaymentSettings.AdditionalFeePercentage);
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
            switch (_ConsolidatePaymentPaymentSettings.TransactMode)
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
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentConsolidatePayment/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentConsolidatePayment";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new ConsolidatePaymentPaymentSettings
            {
                TransactMode = TransactMode.Pending
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Instructions", "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.TiendaId", "TiendaId");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.OrdenId", "OrdenId");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.ClienteId", "ClienteId");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.BancoEmisorId", "EmisorId");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.BancoReceptorId", "ReceptorId");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.Referencia", "Referencia");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.BancoEmisor", "Banco");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.BancoReceptor", "Banco");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.EmailEmisor", "Email Emisor");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.MetodoPago", "Metodo de Pago");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Tienda", "Tienda");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.MontoTotalOrden", "Monto Total");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.FechaRegistro", "F. Creado");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.UltimaActualizacion", "F. Actualizado");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Consolidate", "Consolidado"); 
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Pay", "Pagar"); 
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.CodigoMoneda", "CodMoneda");


            _context.Install();
            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ConsolidatePaymentPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Instructions");
          

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.TiendaId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.OrdenId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.ClienteId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.BancoEmisorId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.BancoReceptorId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.Referencia");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.BancoEmisor");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.BancoReceptor");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.EmailEmisor");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.MetodoPago");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Tienda");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.MontoTotalOrden");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.FechaRegistro");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.UltimaActualizacion");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Consolidate");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Pay");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.ConsolidatePayment.Fields.CodigoMoneda");

            _context.Uninstall();
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
            get { return _localizationService.GetResource("Plugins.Payments.ConsolidatePayment.PaymentMethodDescription"); }
        }

        #endregion

    }
}