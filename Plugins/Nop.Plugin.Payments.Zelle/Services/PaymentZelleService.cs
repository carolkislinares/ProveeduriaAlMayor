using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Zelle.Domain;
using Nop.Services.Orders;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Nop.Plugin.Payments.Zelle.Services
{
    /// <summary>
    /// Store pickup point service
    /// </summary>
    public partial class PaymentZelleService : IPaymentZelleService
    {
        #region Constants

        /// <summary>
        /// Cache key for pickup points
        /// </summary>
        /// <remarks>
        /// {0} : page index
        /// {1} : page size
        /// {2} : current store ID
        /// </remarks>
        private const string ZELLE_ALL_KEY = "Nop.paymentzelle.all-{0}-{1}";
        private const string ZELLE_PATTERN_KEY = "Nop.paymentzelle.";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<PaymentZelle> _zelleRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="BankRepository">Store pickup point repository</param>
        public PaymentZelleService(ICacheManager cacheManager,
            IRepository<PaymentZelle> zelleRepository,
            IRepository<Order> orderRepository,
           IOrderService orderService,
           IOrderProcessingService orderProcessingService)
        {
            this._cacheManager = cacheManager;
            this._zelleRepository = zelleRepository;
            this._orderRepository = orderRepository;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
        }
        #endregion

        #region Method

        public void DeletePaymentZelle(PaymentZelle zelle)
        {
            try
            {
                if (zelle == null)
                    throw new ArgumentNullException(nameof(zelle));

                _zelleRepository.Delete(zelle);
                _cacheManager.RemoveByPattern(ZELLE_PATTERN_KEY);

            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la eliminar pago zelle: " + ex.Message, ex);
            }
        }

        public IPagedList<PaymentZelle> GetAllPaymentZelle(string referencia, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {

                var key = string.Format(ZELLE_ALL_KEY, pageIndex, pageSize);

                var zelle = _zelleRepository.Table;

                var orders = _orderRepository.Table;
                if (!string.IsNullOrWhiteSpace(referencia))
                    zelle = zelle.Where(o => o.ReferenceNumber.Contains(referencia));

                return _cacheManager.Get(key, () =>
                {

                    var listTransferzelle = from tr in zelle
                                                //join or in orders on tr.OrderId equals or.Id
                                                //join br in banks on tr.ReceiverBankId equals br.Id
                                                ////join be in banks on tr.IssuingBankId equals be.Id
                                            select tr;
                    return new PagedList<PaymentZelle> (listTransferzelle, pageIndex, pageSize);
                });
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la lista zelle: " + ex.Message, ex);
            }
        }

        public PaymentZelle GetPaymentZelleById(int transferId)
        {
            try
            {
                if (transferId == 0)
                    return null;

                var zelle = _zelleRepository.Table;
                var orders = _orderRepository.Table;
                var listTransferzelle = from tr in zelle
                                            //join or in orders on tr.OrderId equals or.Id
                                            //join br in banks on tr.ReceiverBankId equals br.Id
                                            ////join be in banks on tr.IssuingBankId equals be.Id
                                        where tr.Id.Equals(transferId)
                                        select tr; 

                return listTransferzelle.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la transferencia Zelle: " + ex.Message, ex);
            }
        }

        public PaymentZelle GetPaymentZelleByOrderId(int orderid)
        {
            try
            {
                if (orderid == 0)
                    return null;

                var zelle = _zelleRepository.Table;
                var orders = _orderRepository.Table;
                var listTransferzelle = from tr in zelle
                                            //join or in orders on tr.OrderId equals or.Id
                                            //join br in banks on tr.ReceiverBankId equals br.Id
                                            ////join be in banks on tr.IssuingBankId equals be.Id
                                        where tr.OrderId.Equals(orderid)
                                        select tr;
                return listTransferzelle.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la transferencia Zelle: " + ex.Message, ex);
            }
        }

        public void InsertPaymentZelle(PaymentZelle zelle)
        {
            try
            {
                if (zelle == null)
                    throw new ArgumentNullException(nameof(zelle));

                var order = _orderService.GetOrderById(zelle.OrderId);
                if (order == null)
                    throw new ArgumentNullException(nameof(zelle));

                zelle.OrderTotal = order.OrderTotal*order.CurrencyRate;
                zelle.PaymentStatusOrder = order.PaymentStatusId;

                _zelleRepository.Insert(zelle);
                _cacheManager.RemoveByPattern(ZELLE_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar el pago Zelle: " + ex.Message, ex);
            }
        }

        public void UpdatePaymentZelle(PaymentZelle zelle)
        {
            try
            {
                if (zelle == null)
                    throw new ArgumentNullException(nameof(zelle));

                var paymentZelle = GetPaymentZelleById(zelle.Id);

                if (paymentZelle == null)
                    throw new ArgumentNullException(nameof(paymentZelle));

                paymentZelle.ReferenceNumber = zelle.ReferenceNumber;
                paymentZelle.IssuingEmail = zelle.IssuingEmail;

                _zelleRepository.Update(paymentZelle);
                _cacheManager.RemoveByPattern(ZELLE_PATTERN_KEY);

            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar la transferencia Zelle: " + ex.Message, ex);
            }
        }

        public void UpdateStateOrderPaymentZelle(int orderId)
        {
            try
            {
                if (orderId == 0)
                {
                    throw new ArgumentNullException(nameof(orderId));
                }
                var order = _orderService.GetOrderById(orderId);
                if (order == null)
                    throw new ArgumentNullException(nameof(orderId));

                _orderProcessingService.MarkOrderAsPaid(order);

                var paymentZelle = GetPaymentZelleByOrderId(orderId);
                if (paymentZelle == null)
                    throw new ArgumentNullException(nameof(orderId));

                var order2 = _orderService.GetOrderById(orderId);
                if (order2 == null)
                    throw new ArgumentNullException(nameof(orderId));

                paymentZelle.PaymentStatusOrder = (int)order2.PaymentStatus;
                _zelleRepository.Update(paymentZelle);
                _cacheManager.RemoveByPattern(ZELLE_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar la Orden: " + ex.Message, ex);
            }
        }


        PaymentZelle IPaymentZelleService.GetPaymentZelleByReference(string referencenumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(referencenumber))
                    return null;

                var zelle = _zelleRepository.Table;
                var orders = _orderRepository.Table;
                var listTransferzelle = from tr in zelle
                                            //join or in orders on tr.OrderId equals or.Id
                                            //join br in banks on tr.ReceiverBankId equals br.Id
                                            ////join be in banks on tr.IssuingBankId equals be.Id
                                        where tr.ReferenceNumber.Equals(referencenumber)
                                        select tr;
                return listTransferzelle.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la transferencia Zelle: " + ex.Message, ex);
            }
        }


        #endregion

    }
}