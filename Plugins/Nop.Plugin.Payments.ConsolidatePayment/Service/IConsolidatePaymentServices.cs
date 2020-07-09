using Nop.Core;
using Nop.Plugin.Payments.ConsolidatePayment.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.ConsolidatePayment.Service
{
    public partial interface IConsolidatePaymentServices
    {

        /// <summary>
        /// Inserts a payment 
        /// </summary>
        /// <param name="bank">bank</param>
        void InsertPayment(Consolidate payment);

        /// <summary>
        /// Get All payment
        /// </summary>
        /// <returns></returns>
        IList<Consolidate> GetPaymentAlls();

        /// <summary>
        /// Get payment by id
        /// </summary>
        /// <returns></returns>
        Consolidate GetPaymentById(int id);

        Consolidate GetPaymentByOrderId(int orderid);

        /// <summary>
        /// Gets a SearchPayment by filter
        /// </summary>
        /// <param name="payment">filter</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>PaymentTransfers</returns>
        IPagedList<Consolidate> SearchPayment(Consolidate payment, int storeId = 0,int pageIndex = 0, int pageSize = int.MaxValue);


        /// <summary>
        /// Update a Payment
        /// /// <param name="payment">Payment</param>
        void UpdatePayment(Consolidate payment);

        /// <summary>
        /// Update state a Payment
        /// /// <param name="id">idPayment</param>
        void UpdateStatePayment(int id);
    }
}
