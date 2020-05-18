using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Payments.Transfer.Domain;

namespace Nop.Plugin.Payments.Transfer.Services
{
    /// <summary>
    /// Store pickup point service
    /// </summary>
    public partial class BankService : IBankService
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
        private const string BANK_ALL_KEY = "Nop.bank.all-{0}-{1}-{2}";
        private const string BANK_PATTERN_KEY = "Nop.bank.";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Bank> _bankRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="BankRepository">Store pickup point repository</param>
        public BankService(ICacheManager cacheManager,
            IRepository<Bank> bankRepository)
        {
            this._cacheManager = cacheManager;
            this._bankRepository = bankRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Pickup points</returns>
        public virtual IPagedList<Bank> GetAllBanks(string bank,int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                var key = string.Format(BANK_ALL_KEY, pageIndex, pageSize, storeId);
                return _cacheManager.Get(key, () =>
                {
                    var query = _bankRepository.Table;
                    if (storeId > 0)
                        query = query.Where(b => b.StoreId == storeId || b.StoreId == 0);
                    if (!string.IsNullOrWhiteSpace(bank))
                        query = query.Where(b => b.Name.Contains(bank));

                    query = query.OrderBy(b => b.Id).ThenBy(b => b.Name);

                    return new PagedList<Bank>(query, pageIndex, pageSize);
                });
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los bancos:: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="bankId">Pickup point identifier</param>
        /// <returns>Pickup point</returns>
        public virtual Bank GetBankById(int bankId)
        {
            try
            {
                if (bankId == 0)
                    return null;

                return _bankRepository.GetById(bankId);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener el banco: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="bank">Pickup point</param>
        public virtual void InsertBank(Bank bank)
        {
            try
            {
                if (bank == null)
                    throw new ArgumentNullException(nameof(bank));

                _bankRepository.Insert(bank);
                _cacheManager.RemoveByPattern(BANK_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar el banco: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates the pickup point
        /// </summary>
        /// <param name="bank">Pickup point</param>
        public virtual void UpdateBank(Bank bank)
        {
            try
            {
                if (bank == null)
                    throw new ArgumentNullException(nameof(bank));

                _bankRepository.Update(bank);
                _cacheManager.RemoveByPattern(BANK_PATTERN_KEY);

            }
            catch (Exception ex)
            {
                throw new NopException("Error al actualizar el banco: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes a pickup point
        /// </summary>
        /// <param name="bank">Pickup point</param>
        public virtual void DeleteBank(Bank bank)
        {
            try
            {
                if (bank == null)
                    throw new ArgumentNullException(nameof(bank));

                _bankRepository.Delete(bank);
                _cacheManager.RemoveByPattern(BANK_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al eliminar el banco: " + ex.Message, ex);
            }
        }

        public IPagedList<Bank> GetBankReceiver(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                var key = string.Format(BANK_ALL_KEY, pageIndex, pageSize, storeId);
                return _cacheManager.Get(key, () =>
                {
                    var query = _bankRepository.Table;
                    if (storeId > 0)
                        query = query.Where(b => b.StoreId == storeId || b.StoreId == 0 && b.IsReceiver);
                    query = query.Where(b => b.IsReceiver).OrderBy(b => b.Name).ThenBy(b => b.Name);

                    return new PagedList<Bank>(query, pageIndex, pageSize);
                });
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los bancos receptores: " + ex.Message, ex);
            }
        }

        public IList<Bank> GetBankReceiver()
        {
            try
            {
                var query = _bankRepository.Table;
                query = query.Where(b => b.IsReceiver);
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los bancos receptores: " + ex.Message, ex);
            }
        }

        public IList<Bank> GetBankAlls()
        {
            try
            {
                var query = _bankRepository.Table;
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los bancos: " + ex.Message, ex);
            }
        }

        #endregion
    }
}