using Nop.Core;
using Nop.Plugin.Payments.Transfer.Domain;
using Nop.Plugin.Payments.Transfer.Models;

namespace Nop.Plugin.Payments.Transfer.Services
{
    /// <summary>
    ///  Bank service interface
    /// </summary>
    public partial interface IPaymentTransferService
    {
        /// <summary>
        /// Gets all PaymentTransfer
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>PaymentTransfers</returns>
        IPagedList<PaymentTransfer> GetAllPaymentTransfer(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a PaymentTransfer by id
        /// </summary>
        /// <param name="transferId">transfer identifier</param>
        /// <returns>PaymentTransfer</returns>
        PaymentTransfer GetPaymentTransferById(int transferId);

        /// <summary>
        /// Gets a PaymentTransfer by reference
        /// </summary>
        /// <param name="referencenumber">transfer reference number</param>
        /// <returns>PaymentTransfer</returns>
        PaymentTransfer GetPaymentTransferByReference(string referencenumber);

        /// <summary>
        /// Inserts a PaymentTransfer
        /// </summary>
        /// <param name="transfer">Transfer</param>
        void InsertPaymentTransfer(PaymentTransfer transfer);

        /// <summary>
        /// Deletes a PaymentTransfer
        /// </summary>
        /// <param name="transfer">Transfer</param>
        void DeletePaymentTransfer(PaymentTransfer transfer);

        /// <summary>
        /// Update State OrderId a PaymentTransfer
        /// /// <param name="orderId">Order identifier</param>
        void UpdateStatePaymentTransfer(int orderId);


        /// <summary>
        /// Gets a PaymentTransfer by orderid
        /// </summary>
        /// <param name="orderid">order identifier</param>
        /// <returns>PaymentTransfer</returns>
        PaymentTransfer GetPaymentTransferByOrderId(int orderid);

        /// <summary>
        /// Gets a PaymentTransfer by filter
        /// </summary>
        /// <param name="transfer">filter</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>PaymentTransfers</returns>
        IPagedList<PaymentTransfer> SearchTransfer(PaymentTransferSearch transfer, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Update a PaymentTransfer
        /// /// <param name="transfer">Transfer</param>
        void UpdatePaymentTransfer(PaymentTransfer transfer);

    }
}