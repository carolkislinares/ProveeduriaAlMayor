using Nop.Core;
using Nop.Plugin.Payments.Transfer.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.Transfer.Services
{
    /// <summary>
    ///  Bank service interface
    /// </summary>
    public partial interface IBankService
    {
        /// <summary>
        /// Gets all banks
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Banks</returns>
        IPagedList<Bank> GetAllBanks(string bank, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a bank
        /// </summary>
        /// <param name="bankId">bank identifier</param>
        /// <returns>Bank</returns>
        Bank GetBankById(int bankId);

        /// <summary>
        /// Gets a bank receiver
        /// </summary>
        /// <param name="bankId">bank identifier</param>
        /// <returns>Bank</returns>
        IPagedList<Bank> GetBankReceiver(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<Bank> GetBankReceiver();
        IList<Bank> GetBankAlls();

        /// <summary>
        /// Inserts a bank
        /// </summary>
        /// <param name="bank">bank</param>
        void InsertBank(Bank bank);

        /// <summary>
        /// Updates a bank
        /// </summary>
        /// <param name="bank">Bank</param>
        void UpdateBank(Bank bank);

        /// <summary>
        /// Deletes a bank
        /// </summary>
        /// <param name="bank">Bank</param>
        void DeleteBank(Bank bank);
    }
}