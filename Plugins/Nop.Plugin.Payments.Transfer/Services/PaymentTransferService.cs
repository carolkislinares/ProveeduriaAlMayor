using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Payments.Transfer.Domain;
using Nop.Plugin.Payments.Transfer.Models;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Transfer.Services
{
    /// <summary>
    /// Store pickup point service
    /// </summary>
    public partial class PaymentTransferService : IPaymentTransferService
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
        private const string TRANSFER_ALL_KEY = "Nop.paymenttransfer.all-{0}-{1}";
        private const string TRANSFER_SEARCH_KEY = "Nop.paymenttransfer.all-{0}-{1}-{2}";
        private const string TRANSFER_PATTERN_KEY = "Nop.paymenttransfer.";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<PaymentTransfer> _transferRepository;
        private readonly IRepository<Order> _orderRepository;

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IBankService _bankService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="BankRepository">Store pickup point repository</param>
        public PaymentTransferService(ICacheManager cacheManager,
            IRepository<PaymentTransfer> transferRepository,
           IOrderProcessingService orderProcessingService, 
           IOrderService orderService,
           IBankService bankService, 
           IRepository<Order> orderRepository)
        {
            this._cacheManager = cacheManager;
            this._transferRepository = transferRepository;
            this._orderRepository = orderRepository;
            this._bankService = bankService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
           
        }
        #endregion

        #region Method
        public void DeletePaymentTransfer(PaymentTransfer transfer)
        {
            if (transfer == null)
                throw new ArgumentNullException(nameof(transfer));

            _transferRepository.Delete(transfer);
            _cacheManager.RemoveByPattern(TRANSFER_PATTERN_KEY);
        }



        public IPagedList<PaymentTransfer> GetAllPaymentTransfer(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {

                var key = string.Format(TRANSFER_ALL_KEY, pageIndex, pageSize);

                var transfers = _transferRepository.Table;
                //var banks = _bankRepository.Table;
                var orders = _orderRepository.Table;

                return _cacheManager.Get(key, () =>
                {

                   var listTransfer= from tr in transfers
                                         //         join or in orders on tr.OrderId equals or.Id
                                         //                   //join br in banks on tr.ReceiverBankId equals br.Id
                                         //                   ////join be in banks on tr.IssuingBankId equals be.Id
                                     select tr;
                    return new PagedList<PaymentTransfer>(listTransfer, pageIndex, pageSize);
                });
            
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la lista: " + ex.Message, ex);
            }
        }

        public PaymentTransfer GetPaymentTransferById(int transferId)
        {
            try
            {
                if (transferId == 0)
                    return null;

                var transfers = _transferRepository.Table;
                //var banks = _bankRepository.Table;
                //var orders = _orderRepository.Table;

                var transferInfo = from tr in transfers
                //                   //join or in orders on tr.OrderId equals or.Id
                //                   //join br in banks on tr.ReceiverBankId equals br.Id
                //                   //join be in banks on tr.IssuingBankId equals be.Id
                                  where tr.Id.Equals(transferId)
                                   select tr;
                return transferInfo.FirstOrDefault();

                //var transferInfo = _dbContext.QueryFromSql<PaymentTransferList>("EXEC[PaymentTransferLoad]").ToList();

                //return transferInfo.Where(c=>c.ID.Equals(transferId)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la transferencia: " + ex.Message, ex);
            }
        }

        public PaymentTransfer GetPaymentTransferByReference(string referencenumber)
        {
            try
            {
                var transfers = _transferRepository.Table;
                //var banks = _bankRepository.Table;
                //var orders = _orderRepository.Table;

                var transferInfo = from tr in transfers
                //join or in orders on tr.OrderId equals or.Id
                //join br in banks on tr.ReceiverBankId equals br.Id
                //join be in banks on tr.IssuingBankId equals be.Id
                where tr.ReferenceNumber.Equals(referencenumber)

                                   select tr;
                return transferInfo.FirstOrDefault();


                //var transferInfo = _dbContext.QueryFromSql<PaymentTransferList>("EXEC [PaymentTransferLoad]").ToList();
                //return transferInfo.Where(c => c.referencia.Equals(referencenumber)).FirstOrDefault();

            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la transferencia: " + ex.Message, ex);
            }
        }

        public PaymentTransfer GetPaymentTransferByOrderId(int orderid)
        {
            try
            {
                if (orderid == 0)
                    return null;

                var transfers = _transferRepository.Table;
                var transferInfo = from tr in transfers
                                       //join or in orders on tr.OrderId equals or.Id
                                       //join br in banks on tr.ReceiverBankId equals br.Id
                                       //join be in banks on tr.IssuingBankId equals be.Id
                                   where tr.OrderId.Equals(orderid)
                                   select tr;
                return transferInfo.FirstOrDefault();
                //var transferInfo = _dbContext.QueryFromSql<PaymentTransferList>("EXEC[PaymentTransferLoad]").ToList();
                //return transferInfo.Where(c => c.orden.Equals(orderid)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener la transferencia de la orden: " + ex.Message, ex);
            }
        }
        public void InsertPaymentTransfer(PaymentTransfer transfer)
        {
            try
            {
                if (transfer == null)
                    throw new ArgumentNullException(nameof(transfer));

                var order = _orderService.GetOrderById(transfer.OrderId);
                if (order == null)
                    throw new ArgumentNullException(nameof(transfer));

                var banks = _bankService.GetBankAlls();
                if (banks == null)
                    throw new ArgumentNullException(nameof(transfer));

                transfer.ReceiverBankName = banks.Where(x => x.Id.Equals(transfer.ReceiverBankId)).FirstOrDefault().Name;
                transfer.IssuingBankName = banks.Where(x => x.Id.Equals(transfer.IssuingBankId)).FirstOrDefault().Name;
                transfer.OrderTotal = order.OrderTotal;
                transfer.PaymentStatusOrder = order.PaymentStatusId;

                _transferRepository.Insert(transfer);
                _cacheManager.RemoveByPattern(TRANSFER_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar la transferencia: " + ex.Message, ex);
            }
        }

        public void UpdateStatePaymentTransfer(int orderId)
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

                var paymentTransfer = GetPaymentTransferByOrderId(orderId);

                if (paymentTransfer == null)
                    throw new ArgumentNullException(nameof(orderId));

                var order2 = _orderService.GetOrderById(orderId);
                    if (order2 == null)
                        throw new ArgumentNullException(nameof(orderId));

                paymentTransfer.PaymentStatusOrder = (int)order2.PaymentStatus;
                _transferRepository.Update(paymentTransfer);
                    _cacheManager.RemoveByPattern(TRANSFER_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar la Orden: " + ex.Message, ex);
            }
        }

        public IPagedList<PaymentTransfer> SearchTransfer(PaymentTransferSearch transfer, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                var key = string.Format(TRANSFER_SEARCH_KEY, transfer, pageIndex, pageSize);
                if (transfer == null)
                    throw new ArgumentNullException(nameof(transfer));

                var transfers = _transferRepository.Table;
               
                if (transfer.orden > 0)
                    transfers = transfers.Where(o => o.OrderId == transfer.orden);
                if (transfer.BancoEmisorId > 0)
                    transfers = transfers.Where(o => o.IssuingBankId == transfer.BancoEmisorId);

                if (!string.IsNullOrWhiteSpace(transfer.referencia))
                    transfers = transfers.Where(o => o.ReferenceNumber.Contains(transfer.referencia));

                if (transfer.BancoReceptorId > 0)
                    transfers = transfers.Where(o => o.ReceiverBankId == transfer.BancoReceptorId);

                return _cacheManager.Get(key, () =>
                {

                    var listTransfer = from tr in transfers
                                       select tr;
                    return new PagedList<PaymentTransfer>(listTransfer, pageIndex, pageSize);
                });
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar la transferencia: " + ex.Message, ex);
            }
        }

        public void UpdatePaymentTransfer(PaymentTransfer transfer)
        {
            try
            {
                if (transfer == null)
                    throw new ArgumentNullException(nameof(transfer));

                var paymentTransfer = GetPaymentTransferById(transfer.Id);

                if (paymentTransfer == null)
                    throw new ArgumentNullException(nameof(paymentTransfer));

                paymentTransfer.IssuingBankId = transfer.IssuingBankId;
                paymentTransfer.ReceiverBankId = transfer.ReceiverBankId;
                paymentTransfer.ReferenceNumber = transfer.ReferenceNumber;

                _transferRepository.Update(paymentTransfer);
                _cacheManager.RemoveByPattern(TRANSFER_PATTERN_KEY);

            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar la transferencia: " + ex.Message, ex);
            }
        }


        #endregion

    }
}