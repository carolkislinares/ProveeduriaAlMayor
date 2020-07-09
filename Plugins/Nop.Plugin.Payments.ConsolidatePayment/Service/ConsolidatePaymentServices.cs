using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.ConsolidatePayment.Domain;
using Nop.Plugin.Payments.Transfer.Services;
using Nop.Plugin.Payments.Zelle.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.ConsolidatePayment.Service
{
    class ConsolidatePaymentServices : IConsolidatePaymentServices
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
        private const string PAYMENT_ALL_KEY = "Nop.payment.all-{0}-{1}-{2}";
        private const string PAYMENT_PATTERN_KEY = "Nop.payment.";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Consolidate> _paymentRepository;
        private readonly IPaymentTransferService _transferServices;
        private readonly IPaymentZelleService _zelleServices;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IRepository<Order> _orderRepository;

        #endregion
        public ConsolidatePaymentServices(ICacheManager cacheManager,
           IRepository<Consolidate> paymentRepository,
          IOrderProcessingService orderProcessingService,
          IOrderService orderService,
          IRepository<Order> orderRepository,
           IPaymentTransferService transferServices,
           IPaymentZelleService zelleServices)
        {
            this._cacheManager = cacheManager;
            this._paymentRepository = paymentRepository;
            this._orderRepository = orderRepository;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._transferServices = transferServices;
            this._zelleServices = zelleServices;
        }

        public IList<Consolidate> GetPaymentAlls()
        {
            try
            {
                var query = _paymentRepository.Table;
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener todos los pagos: " + ex.Message, ex);
            }
        }

        public Consolidate GetPaymentById(int id)
        {
            try
            {
                if (id == 0)
                    return null;

                var payments = _paymentRepository.Table;
              
                var info = from tr in payments
                                   where tr.Id.Equals(id)
                                   select tr;
                return info.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener el pago: " + ex.Message, ex);
            }
        }


        public Consolidate GetPaymentByOrderId(int orderid)
        {
            try
            {
                if (orderid == 0)
                    return null;

                var payments = _paymentRepository.Table;

                var info = from tr in payments
                           where tr.OrdenId.Equals(orderid)
                           select tr;
                return info.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener el pago de la orden: " + ex.Message, ex);
            }
        }
        public void InsertPayment(Consolidate payment)
        {
            try
            {
                if (payment == null)
                    throw new ArgumentNullException(nameof(payment));

                _paymentRepository.Insert(payment);
                _cacheManager.RemoveByPattern(PAYMENT_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar el pago: " + ex.Message, ex);
            }
        }
     
        public IPagedList<Consolidate> SearchPayment(Consolidate payment, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                var key = string.Format(PAYMENT_ALL_KEY, pageIndex, pageSize, payment.TiendaId);
                return _cacheManager.Get(key, () =>
                {
                    var query = _paymentRepository.Table;

                    if (payment.TiendaId > 0)
                        query = query.Where(b => b.TiendaId == storeId || b.TiendaId == 0);

                    if (payment.OrdenId > 0)
                        query = query.Where(b => b.OrdenId == payment.OrdenId);

                    if (payment.ClienteId > 0)
                        query = query.Where(b => b.ClienteId == payment.ClienteId);

                    if (!string.IsNullOrWhiteSpace(payment.Referencia))
                        query = query.Where(b => b.Referencia.Contains(payment.Referencia));

                    if (!string.IsNullOrWhiteSpace(payment.MetodoPago))
                        query = query.Where(b => b.MetodoPago.Contains(payment.MetodoPago));

                    if (payment.StatusPaymentOrder>0)
                        query = query.Where(b => b.StatusPaymentOrder== payment.StatusPaymentOrder);

                    if (payment.BancoEmisorId > 0)
                        query = query.Where(b => b.BancoEmisorId == payment.BancoEmisorId);

                    if (payment.BancoReceptorId > 0)
                        query = query.Where(b => b.BancoReceptorId == payment.BancoReceptorId);

                    query = query.OrderBy(b => b.Id).ThenBy(b => b.FechaUltimaActualizacion);

                    return new PagedList<Consolidate>(query, pageIndex, pageSize);
                });
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los pagos:: " + ex.Message, ex);
            }
        }

        public void UpdatePayment(Consolidate payment)
        {
            try
            {
                if (payment == null)
                    throw new ArgumentNullException(nameof(payment));

                //if (payment.OrdenId == 0)
                //{
                //    throw new ArgumentNullException(nameof(payment.OrdenId));
                //}
                //var order = _orderService.GetOrderById(payment.OrdenId);
                //if (order == null)
                //    throw new ArgumentNullException(nameof(payment.OrdenId));

                //_orderProcessingService.MarkOrderAsPaid(order);


                //if (paymentTransfer == null)
                //    throw new ArgumentNullException(nameof(payment.OrdenId));

                //var order2 = _orderService.GetOrderById(payment.OrdenId);
                //if (order2 == null)
                //    throw new ArgumentNullException(nameof(payment.OrdenId));

                switch (payment.MetodoPago)
                {
                    case "Payments.Transfer":
                    {
                        try
                        {
                            var paymentTransfer = _transferServices.GetPaymentTransferByOrderId(payment.OrdenId);
                            paymentTransfer.ReceiverBankName = payment.BancoEmisor;
                            paymentTransfer.IssuingBankName = payment.BancoReceptor;
                            paymentTransfer.ReceiverBankId = payment.BancoEmisorId;
                            paymentTransfer.IssuingBankId = payment.BancoReceptorId;
                            paymentTransfer.ReferenceNumber = payment.Referencia;

                            _transferServices.UpdatePaymentTransfer(paymentTransfer);
                        }
                        catch (Exception ex)
                        {
                            throw new NopException("Error en transferencia: " + ex.Message, ex);
                        }
                        break;
                    }
                    default: {
                        try
                        {
                            var zelleTransfer = _zelleServices.GetPaymentZelleByOrderId(payment.OrdenId);
                             zelleTransfer.ReferenceNumber = payment.Referencia;
                             zelleTransfer.IssuingEmail = payment.EmailEmisor;

                            _zelleServices.UpdatePaymentZelle(zelleTransfer);
                        }
                        catch (Exception ex)
                        {
                            throw new NopException("Error en zelle: " + ex.Message, ex);
                        }
                        break;
                    }
                }

                 payment.FechaUltimaActualizacion = DateTime.Now;
                _paymentRepository.Update(payment);
                _cacheManager.RemoveByPattern(PAYMENT_PATTERN_KEY);

            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar el pago: " + ex.Message, ex);
            }
        }

        public void UpdateStatePayment(int id)
        {
            try
            {
                var payment = _paymentRepository.GetById(id);
                if (payment == null)
                    throw new ArgumentNullException(nameof(payment));

                if (payment.OrdenId == 0)
                    throw new ArgumentNullException(nameof(payment.OrdenId));

                var order = _orderService.GetOrderById(payment.OrdenId);
                if (order == null)
                    throw new ArgumentNullException(nameof(payment.OrdenId));

                _orderProcessingService.MarkOrderAsPaid(order);

                var order2 = _orderService.GetOrderById(payment.OrdenId);
                if (order2 == null)
                    throw new ArgumentNullException(nameof(payment.OrdenId));


                switch (payment.MetodoPago)
                {
                    case "Payments.Transfer":
                    {
                        try
                        {
                            var paymentTransfer = _transferServices.GetPaymentTransferByOrderId(payment.OrdenId);
                            if (paymentTransfer == null)
                                throw new ArgumentNullException(nameof(payment.OrdenId));

                                paymentTransfer.PaymentStatusOrder = (int)order2.PaymentStatus;
                               _transferServices.UpdatePaymentTransfer(paymentTransfer);
                        }
                        catch (Exception ex)
                        {
                            throw new NopException("Error en State transferencia: " + ex.Message, ex);
                        }
                        break;
                    }
                    default:
                    {
                        try
                        {
                            var paymentZelle = _zelleServices.GetPaymentZelleByOrderId(payment.OrdenId);
                            if (paymentZelle == null)
                                throw new ArgumentNullException(nameof(payment.OrdenId));

                            paymentZelle.PaymentStatusOrder = (int)order2.PaymentStatus;
                            _zelleServices.UpdatePaymentZelle(paymentZelle);
                        }
                        catch (Exception ex)
                        {
                            throw new NopException("Error en State zelle: " + ex.Message, ex);
                        }
                        break;
                    }
                }

                 payment.FechaUltimaActualizacion = DateTime.Now;
                 payment.StatusPaymentOrder = (int)order2.PaymentStatus;
                _paymentRepository.Update(payment);
                _cacheManager.RemoveByPattern(PAYMENT_PATTERN_KEY);

            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar el status del pago: " + ex.Message, ex);
            }
        }
    }
}
