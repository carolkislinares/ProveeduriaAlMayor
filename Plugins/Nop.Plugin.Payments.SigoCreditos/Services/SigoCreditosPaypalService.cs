using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Payments.SigoCreditos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Services
{
    public partial class SigoCreditosPaypalService : ISigoCreditosPaypalService
    {


        private readonly ICacheManager _cacheManager;
        private readonly IRepository<SigoCreditosPaypal> _SigoCreditosPaypalRepository;

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="BankRepository">Store pickup point repository</param>
        public SigoCreditosPaypalService(ICacheManager cacheManager,
            IRepository<SigoCreditosPaypal> bankRepository)
        {
            this._cacheManager = cacheManager;
            this._SigoCreditosPaypalRepository = bankRepository;
        }

        #endregion

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="bank">Pickup point</param>
        public virtual void InsertSigoCreditosPaypal(SigoCreditosPaypal SCPaypal)
        {
            try
            {
                if (SCPaypal == null)
                    throw new ArgumentNullException(nameof(SCPaypal));

                _SigoCreditosPaypalRepository.Insert(SCPaypal);
               // _cacheManager.RemoveByPattern(BANK_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar el banco: " + ex.Message, ex);
            }
        }


        public IList<SigoCreditosPaypal> GetSigoCreditosPaypalAlls()
        {
            try
            {
                var query = _SigoCreditosPaypalRepository.Table;
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los SigroCreditosPaypal: " + ex.Message, ex);
            }
        }


    }
}
