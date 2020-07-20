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
using Nop.Plugin.Payments.EpagosMercantil.Models;
using Nop.Plugin.Payments.EpagosMercantil.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Globalization;
using RestSharp;

namespace Nop.Plugin.Payments.EpagosMercantil
{
    /// <summary>
    /// Manual payment processor
    /// </summary>Payments.EpagosMercantil
    public class EpagosMercantilPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly EpagosMercantilPaymentSettings _epagosMercantilPaymentSettings;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public EpagosMercantilPaymentProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            EpagosMercantilPaymentSettings EpagosMercantilPaymentSettings,
            IStoreContext storeContext)
        {
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._epagosMercantilPaymentSettings = EpagosMercantilPaymentSettings;
            this._storeContext = storeContext;
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

            ProcessPaymentEpagosMercantil(processPaymentRequest, out result);

            switch (_epagosMercantilPaymentSettings.TransactMode)
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
                _epagosMercantilPaymentSettings.AdditionalFee, _epagosMercantilPaymentSettings.AdditionalFeePercentage);
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

            switch (_epagosMercantilPaymentSettings.TransactMode)
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentEpagosMercantil/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentEpagosMercantil";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new EpagosMercantilPaymentSettings
            {
                TransactMode = TransactMode.Pending
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Instructions", "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.TransactMode", "After checkout mark payment as");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.TransactMode.Hint", "Specify transaction mode.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.EpagosMercantil.PaymentMethodDescription", "Pagos en Bolívares por tarjetas de débito/crédito");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<EpagosMercantilPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Instructions");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.TransactMode");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.Fields.TransactMode.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.EpagosMercantil.PaymentMethodDescription");

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
            get { return _localizationService.GetResource("Plugins.Payments.EpagosMercantil.PaymentMethodDescription"); }
        }

        #endregion

        #region ProcessPaymentEpagosMercantil

        struct EpagosMercantil
        {
            public string URLConnection;
            public string KeyId;
            public string PublicKey;
        }

        public void ProcessPaymentEpagosMercantil(ProcessPaymentRequest processPaymentRequest, out ProcessPaymentResult result)
        {
            try
            {
                ProcessPaymentResult _result = new ProcessPaymentResult();
                EpagosMercantil items = new EpagosMercantil {
                    URLConnection = _settingService.GetSettingByKey("UrlMercantil", "", _storeContext.CurrentStore.Id, true),
                };
             
                string idComercio = "104755";
                string tipotransaccion = "0200";
                string monto = Convert.ToString(processPaymentRequest.OrderTotal).Replace(".", ",");
                string fechavcto = String.Format(processPaymentRequest.CreditCardExpireMonth.ToString() + processPaymentRequest.CreditCardExpireYear.ToString().Substring(2));
                
                //                "Pago+Compra+EcommerceSigo",
                //                processPaymentRequest.CreditCardName.Replace(" ", "+"),
                //                processPaymentRequest.CreditCardNumberId,
                //                processPaymentRequest.CreditCardNumber,
                //                processPaymentRequest.CreditCardCvv2,
                //                processPaymentRequest.CreditCardExpireMonth.ToString("00"),
                //                processPaymentRequest.CreditCardExpireYear, 
                //                "c");

                string apikey = Apikey(idComercio, tipotransaccion, Convert.ToString(processPaymentRequest.OrderTotal).Replace(".", ","),
                                                                     "numerofact" + processPaymentRequest.OrderGuid,
                                                                     processPaymentRequest.CreditCardNumberId,
                                                                      processPaymentRequest.CreditCardName.Replace(" ", "+"),
                                                                      processPaymentRequest.CreditCardNumber,
                                                                       String.Format(processPaymentRequest.CreditCardExpireMonth.ToString() + processPaymentRequest.CreditCardExpireYear.ToString().Substring(2)),
                                                                       processPaymentRequest.CreditCardCvv2, "1");

                var client = new RestClient(items.URLConnection);
                var request = new RestRequest();
                string timeStamp = Stopwatch.GetTimestamp().ToString();
               
                request.Method = Method.POST;
                request.AddHeader("x-ibm-client-id", "ba54c4ba-0aba-48ee-a978-1b78392a40a5");
                request.AddHeader("Apikey", apikey);
                request.AddHeader("Timestamp", timeStamp);
                request.AddHeader("Content-Length", apikey.Length.ToString());
                request.AddHeader("Host", "apimbu.mercantilbanco.com:9443");
                request.AddHeader("Accept", "application/json");
                string strJSONContent = "{ \r\n  \"HEADER_PAGO_REQUEST\": { \r\n \"IDENTIFICADOR_UNICO_GLOBAL\": \"900\"," +
                                                                            " \r\n \"IDENTIFICACION_CANAL\": \"06\", \r\n  " +
                                                                            "  \"SIGLA_APLICACION\": \"APIC\", \r\n    " +
                                                                            "\"IDENTIFICACION_USUARIO\": \"66\", \r\n   " +
                                                                            " \"DIRECCION_IP_CONSUMIDOR\": \"192.237.245.234\", \r\n  " +
                                                                            "  \"DIRECCION_IP_CLIENTE\": \"200.3.1.8\", \r\n   " +
                                                                            " \"FECHA_ENVIO_MENSAJE\": \""+DateTime.Now.ToString("YYYYMMDD") +"\", \r\n  " +
                                                                            "  \"HORA_ENVIO_MENSAJE\": \""+DateTime.Now.ToString("hhmmss") +"\", \r\n" +
                                                                            "    \"ATRIBUTO_PAGINEO\": \"N\", \r\n  " +
                                                                            "  \"CLAVE_BUSQUEDA\": \"\""+string.Empty+", \r\n  " +
                                                                            "  \"CANTIDAD_REGISTROS\": 0 \r\n  }," +
                                         "\r\n  \"BODY_PAGO_REQUEST\": { \r\n    " +
                                                                            "\"IDENTIFICADOR_COMERCIO\": " + idComercio + ", \r\n  " +
                                                                            "\"TIPO_TRANSACCION\": \""+tipotransaccion+"\", \r\n   " +
                                                                            " \"MONTO_TRANSACCION\": " + monto + ", \r\n    " +
                                                                            "\"NUMERO_FACTURA\": 88888888888, \r\n " +
                                                                            "\"IDENTIFICACION_TARJETAHABIENTE\": \""+ processPaymentRequest.CreditCardNumberId + "\", \r\n  " +
                                                                            "\"NOMBRE_TARJETAHABIENTE\": \""+ processPaymentRequest.CreditCardName + "\", \r\n   " +
                                                                            "\"NUMERO_TARJETA\": \"" + processPaymentRequest.CreditCardNumber + "\", \r\n  " +
                                                                             "\"FECHA_VENCIMIENTO_TARJETA\": "+fechavcto+", \r\n\t\"CODIGO_SEGURIDAD_TARJETA\": "+ processPaymentRequest.CreditCardCvv2 + ", " +
                                                                             "\r\n    \"NUMERO_LOTE\": \"1\" \r\n  } \r\n} \r\n";



                //var json = {
                //    "merchant_identify": {
                //                                    "integratorId": 31,
                //                                    "merchantId": 150332,
                //                                    "terminalId":"abcde"},
                //             "client_identify": {
                //                                "ipaddress": "10.0.0.1",
                //                                "browser_agent": "Chrome 18.1.3",
                //                                "mobile": {"manufacturer": "Samsung"}
                //                               },
                //           "transaction": {
                //                            "trx_type": "compra",
                //                            "payment_method":"tdc",
                //                            "card_number":"501878200048646634",
                //                            "customer_id":"V18780283",
                //                            "invoice_number": "1231243",
                //                            "expiration_date":"2021/11",
                //                            "cvv": "GYlTecZmlHYu7KFTeDaWCQ==",
                //                            "currency":"ves",
                //                            "amount": 30.11}
                //             }"



                request.Parameters.Clear();
                request.AddParameter("application/json", strJSONContent, ParameterType.RequestBody);

                var response = client.Execute(request);



                //byte[] __postDataStream = Encoding.UTF8.GetBytes(PostData.Replace(" ", string.Empty));

                //__webrequest.Method = "POST";
                //__webrequest.ContentType = "application/json";
                //__webrequest.ContentLength = __postDataStream.Length;
                //__webrequest.Headers.Add("x-ibm-client-id");
                //Stream __requestStream = __webrequest.GetRequestStream();
                //__requestStream.Write(__postDataStream, 0, __postDataStream.Length);
                //__requestStream.Close();

                //// response
                //WebResponse __webresponse = __webrequest.GetResponse();
                //Stream dataStream = __webresponse.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                //string responseFromServer = reader.ReadToEnd();

                //EpagosMercantilRSModel __instaRs = JsonConvert.DeserializeObject<EpagosMercantilRSModel>(responseFromServer) as EpagosMercantilRSModel;
                //if (__instaRs.Success == true)
                //{
                //    _result.AuthorizationTransactionId = " Código del pago:" + __instaRs.Id;
                //    _result.AuthorizationTransactionCode = "Número de referencia del pago:" + __instaRs.Reference;
                //    _result.AuthorizationTransactionResult = "Mensaje: " + __instaRs.Message;
                //    _result.CaptureTransactionResult = __instaRs.Voucher;
                //    _result.NewPaymentStatus = PaymentStatus.Paid;
                //    _epagosMercantilPaymentSettings.TransactMode = TransactMode.AuthorizeAndCapture;
                //}
                //else
                //{
                //    string __errorMessage;
                //    switch (Convert.ToInt32(__instaRs.Code))
                //    {
                //        case 400:
                //            __errorMessage = "Error al validar los datos enviados: " + __instaRs.Message;
                //            break;
                //        case 401:
                //            __errorMessage = "Error de autenticación, ha ocurrido un error con las llaves utilizadas. " + __instaRs.Message;
                //            break;
                //        case 403:
                //            __errorMessage = "Pago Rechazado por el banco. " + __instaRs.Message;
                //            break;
                //        case 500:
                //            __errorMessage = "Ha Ocurrido un error interno dentro del servidor: " + __instaRs.Message;
                //            break;
                //        case 503:
                //            __errorMessage = "Ha Ocurrido un error al procesar los parámetros de entrada. Revise los datos enviados y vuelva a intentarlo. " + __instaRs.Message;
                //            break;
                //        default:
                //            __errorMessage = "Lo sentimos, no hemos podido procesar su tarjeta de crédito. El mensaje del banco fue: " + __instaRs.Message;
                //            break;
                //    }
                //    _result.AddError(__errorMessage);
                //}
                result = _result;

            }
            catch (Exception ex)
            {
                throw new NopException("Error al procesar el pago: "+ex.Message, ex);
            }
        }





        /// <summary>
        /// Metodo para generar el apikey donde se conectara el api. 
        /// </summary>
        /// <param name="IDENTIFICADOR_COMERCIO">Código de comercio, este código es entregado por Mercantil Banco y es parte de la afiliación realizada por el comercio</param>
        /// <param name="TIPO_TRANSACCION">código de la transacción que se está realizando, para el caso del pago el código debe ser 0200 y para el reverso el código debe ser 0420</param>
        /// <param name="MONTO_TRANSACCION"> Monto correspondiente a la transacción. Ej. 0000,00</param>
        /// <param name="NUMERO_FACTURA">Número de factura/orden</param>
        /// <param name="IDENTIFICACION_TARJETAHABIENTE"> Identificación del TH, en este campo debe colocarse el número de cedula de identidad en caso ce VE o la identificación del cliente correspondiente a su país de origen</param>
        /// <param name="NOMBRE_TARJETAHABIENTE">Nombre del cliente que aparece en la tarjeta utilizada</param>
        /// <param name="NUMERO_TARJETA">Número de tarjeta sin ningún tipo de separación</param>
        /// <param name="FECHA_VENCIMIENTO_TARJETA">Fecha de vencimiento de la tarjeta en formato MMAA. Ej. 720 => Julio del 2020</param>
        /// <param name="CODIGO_SEGURIDAD_TARJETA">Código de seguridad o CVV numero de tres dígitos que está en la parte posterior de la tarjeta</param>
        /// <param name="NUMERO_LOTE">Numero de control utilizado por la empresa que consume el servicio para su uso interno, el valor debe ser numérico y debe ser mayor a cero</param>
        /// <returns></returns>

        static String Apikey(string IDENTIFICADOR_COMERCIO, string TIPO_TRANSACCION, string MONTO_TRANSACCION, string NUMERO_FACTURA, string IDENTIFICACION_TARJETAHABIENTE,
                             string NOMBRE_TARJETAHABIENTE, string NUMERO_TARJETA, string FECHA_VENCIMIENTO_TARJETA, string CODIGO_SEGURIDAD_TARJETA, string NUMERO_LOTE)
        {
            StringBuilder data = new StringBuilder();
            string timeStamp = Stopwatch.GetTimestamp().ToString();
         

            string claveBanco = "B2CM3rcanti1#";
         
            data.Append("identificador_comercio=" + IDENTIFICADOR_COMERCIO + "&");
            data.Append("tipo_transaccion=" + TIPO_TRANSACCION + "&");
            data.Append("monto_transaccion=" + MONTO_TRANSACCION + "&");
            data.Append("numero_factura=" + NUMERO_FACTURA + "&");
            data.Append("identificacion_tarjetahabiente=" + IDENTIFICACION_TARJETAHABIENTE + "&");
            data.Append("nombre_tarjetahabiente=" + NOMBRE_TARJETAHABIENTE + "&");
            data.Append("numero_tarjeta=" + NUMERO_TARJETA + "&");
            data.Append("fecha_vencimiento_tarjeta=" + FECHA_VENCIMIENTO_TARJETA + "&");
            data.Append("codigo_seguridad_tarjeta=" + CODIGO_SEGURIDAD_TARJETA + "&"); 
            data.Append("numero_lote=" + NUMERO_LOTE + "&timestamp=" + timeStamp + "&secret="+claveBanco);
            string mensaje = data.ToString();

            SHA256 mySha = SHA256Managed.Create();
            byte[] utf16Key = mySha.ComputeHash(Encoding.GetEncoding("ISO-8859-15").GetBytes(mensaje));

            StringBuilder hex = new StringBuilder(utf16Key.Length * 2);
            foreach (byte b in utf16Key)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            // String apikey = hex.ToString();
            return hex.ToString();

        }


        #endregion
    }
}