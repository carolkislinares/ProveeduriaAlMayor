using Nop.Core;
using Nop.Plugin.Payments.Zelle.Domain;

namespace Nop.Plugin.Payments.Zelle.Services
{
    /// <summary>
    ///  PaymentZelle service interface
    /// </summary>
    public partial interface IPaymentZelleService
    {
        /// <summary>
        /// Gets all PaymentZelle
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>PaymentZelle</returns>
        IPagedList<PaymentZelle> GetAllPaymentZelle(string referencia, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a PaymentZelle by id
        /// </summary>
        /// <param name="zelleid">zelle identifier</param>
        /// <returns>PaymentZelle</returns>
        PaymentZelle GetPaymentZelleById(int zelleid);



        /// <summary>
        /// Gets a PaymentZelle by orderid
        /// </summary>
        /// <param name="orderid">order identifier</param>
        /// <returns>PaymentZelle</returns>
        PaymentZelle GetPaymentZelleByOrderId(int orderid);

        /// <summary>
        /// Gets a PaymentZelle by reference
        /// </summary>
        /// <param name="referencenumber">transfer reference number</param>
        /// <returns>Bank</returns>
        PaymentZelle GetPaymentZelleByReference(string referencenumber);

        /// <summary>
        /// Inserts a PaymentZelle
        /// </summary>
        /// <param name="zelle">Zelle</param>
        void InsertPaymentZelle(PaymentZelle zelle);

        /// <summary>
        /// Deletes a PaymentZelle
        /// </summary>
        /// <param name="zelle">Zelle</param>
        void DeletePaymentZelle(PaymentZelle zelle);

        /// <summary>
        /// Update OrderId a PaymentZelle
        /// /// <param name="orderId">Order identifier</param>
        void UpdateStateOrderPaymentZelle(int orderId);

        /// <summary>
        /// Update a PaymentZelle
        /// /// <param name="zelle">Zelle</param>
        void UpdatePaymentZelle(PaymentZelle zelle);
    }
}