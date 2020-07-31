using Nop.Core;
using Nop.Plugin.Payments.SigoCreditos.Domain;
using Nop.Plugin.Payments.SigoCreditos.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Services
{
    public partial interface ISigoCreditosPaypalService
    {

        /// <summary>
        /// Inserts a bank
        /// </summary>
        /// <param name="bank">bank</param>
        void InsertSigoCreditosPaypal(SigoCreditosPaypal SCPaypal);

        IList<SigoCreditosPaypal> GetSigoCreditosPaypalAlls();


        IPagedList<ClienteAbonoModel> SearchAbonosClientes(ClienteAbonoModel payment, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    }
}
