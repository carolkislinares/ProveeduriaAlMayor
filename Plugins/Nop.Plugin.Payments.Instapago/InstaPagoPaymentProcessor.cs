using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using System.Web;
using Nop.Plugin.Payments.InstaPago.Models;
using Nop.Plugin.Payments.InstaPago.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;

namespace Nop.Plugin.Payments.InstaPago
{
    /// <summary>
    /// Manual payment processor
    /// </summary>Payments.InstaPago
    public class InstaPagoPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly InstaPagoPaymentSettings _instaPagoPaymentSettings;

        #endregion

        #region Ctor

        public InstaPagoPaymentProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            InstaPagoPaymentSettings instaPagoPaymentSettings)
        {
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._instaPagoPaymentSettings = instaPagoPaymentSettings;
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

            ProcessPaymentInstapago(processPaymentRequest, out result);

            switch (_instaPagoPaymentSettings.TransactMode)
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
                _instaPagoPaymentSettings.AdditionalFee, _instaPagoPaymentSettings.AdditionalFeePercentage);
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

            switch (_instaPagoPaymentSettings.TransactMode)
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
                ExpireYear = form["ExpireYear"],
                DocumentNumber = form["DocumentNumber"],
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
                CreditCardCvv2 = form["CardCode"],
                CreditCardNumberId = form["DocumentNumber"]
            };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentInstaPago/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentInstaPago";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new InstaPagoPaymentSettings
            {
                TransactMode = TransactMode.Pending
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Instructions", "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Fields.TransactMode", "After checkout mark payment as");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.Fields.TransactMode.Hint", "Specify transaction mode.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.InstaPago.PaymentMethodDescription", "Pagos en Bolívares por tarjetas de débito/crédito");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<InstaPagoPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Instructions");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Fields.TransactMode");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.Fields.TransactMode.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.InstaPago.PaymentMethodDescription");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType {
            get { return RecurringPaymentType.Manual; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType {
            get { return PaymentMethodType.Standard; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.InstaPago.PaymentMethodDescription"); }
        }

        #endregion

        #region ProcessPaymentInstapago

        struct InstaPago
        {
            public string URLConnection;
            public string KeyId;
            public string PublicKey;
        }
        public void ProcessPaymentInstapago(ProcessPaymentRequest processPaymentRequest, out ProcessPaymentResult result)
        {
            try
            {
                ProcessPaymentResult _result = new ProcessPaymentResult();
                InstaPago items;
                using (StreamReader r = new StreamReader("InstaPagosettings.json"))
                {
                    string json = r.ReadToEnd();
                     items = JsonConvert.DeserializeObject<InstaPago>(json);
                }

                WebRequest __webrequest = WebRequest.Create(items.URLConnection) as HttpWebRequest;

                string PostData =
                String.Format("KeyId={0}&PublicKeyId={1}&Amount={2}&Description={3}&CardHolder={4}&CardHolderId={5}&CardNumber={6}&CVC={7}&ExpirationDate={8}/{9}&StatusId={10}",
                                items.KeyId,
                                items.PublicKey,
                                processPaymentRequest.OrderTotal,
                                "Pago+Compra+EcommerceSigo",
                                processPaymentRequest.CreditCardName.Replace(" ", "+"),
                                processPaymentRequest.CreditCardNumberId,
                                processPaymentRequest.CreditCardNumber,
                                processPaymentRequest.CreditCardCvv2,
                                processPaymentRequest.CreditCardExpireMonth.ToString("00"),
                                processPaymentRequest.CreditCardExpireYear,
                                (int)StatusTransactInstaPago.Pagar);

                byte[] __postDataStream = Encoding.UTF8.GetBytes(PostData.Replace(" ", string.Empty));

                __webrequest.Method = "POST";
                __webrequest.ContentType = "application/x-www-form-urlencoded";
                __webrequest.ContentLength = __postDataStream.Length;
                Stream __requestStream = __webrequest.GetRequestStream();
                __requestStream.Write(__postDataStream, 0, __postDataStream.Length);
                __requestStream.Close();

                // response
                WebResponse __webresponse = __webrequest.GetResponse();
                Stream dataStream = __webresponse.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                InstaPagoRSModel __instaRs = JsonConvert.DeserializeObject<InstaPagoRSModel>(responseFromServer) as InstaPagoRSModel;
                if (__instaRs.Success == true)
                {
                    _result.AuthorizationTransactionId = " Código del pago:" + __instaRs.Id;
                    _result.AuthorizationTransactionCode = "Número de referencia del pago:" + __instaRs.Reference;
                    _result.AuthorizationTransactionResult = "Mensaje: " + __instaRs.Message;
                    _result.CaptureTransactionResult = __instaRs.Voucher;
                    _result.NewPaymentStatus = PaymentStatus.Paid;
                    _instaPagoPaymentSettings.TransactMode = TransactMode.AuthorizeAndCapture;
                }
                else
                {
                    string __errorMessage;
                    switch (Convert.ToInt32(__instaRs.Code))
                    {
                        case 400:
                            __errorMessage = "Error al validar los datos enviados: " + __instaRs.Message;
                            break;
                        case 401:
                            __errorMessage = "Error de autenticación, ha ocurrido un error con las llaves utilizadas. " + __instaRs.Message;
                            break;
                        case 403:
                            __errorMessage = "Pago Rechazado por el banco. " + __instaRs.Message;
                            break;
                        case 500:
                            __errorMessage = "Ha Ocurrido un error interno dentro del servidor: " + __instaRs.Message;
                            break;
                        case 503:
                            __errorMessage = "Ha Ocurrido un error al procesar los parámetros de entrada. Revise los datos enviados y vuelva a intentarlo. " + __instaRs.Message;
                            break;
                        default:
                            __errorMessage = "Lo sentimos, no hemos podido procesar su tarjeta de crédito. El mensaje del banco fue: " + __instaRs.Message;
                            break;
                    }
                    _result.AddError(__errorMessage);
                }
                result = _result;

            }
            catch (Exception ex)
            {
                throw new NopException("Error al procesar el pago: "+ex.Message, ex);
            }
        }

        #endregion
    }
}