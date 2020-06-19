using Nop.Plugin.Payments.SigoCreditos.Domain;
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

    }
}
