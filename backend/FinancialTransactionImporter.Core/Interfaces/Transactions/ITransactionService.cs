using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialTransactionImporter.Core.DTOs.Transactions;
using FinancialTransactionImporter.Core.Interfaces.Transactions;
using FinancialTransactionImporter.Core.Library;
using FinancialTransactionImporter.Core.Models;
using Microsoft.AspNetCore.Http;

namespace FinancialTransactionImporter.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionUploadResult> UploadTransactionsAsync(IFormFile file);
        Task<PagedResult<Transaction>> GetTransactionsAsync(int pageNumber, int pageSize);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<Transaction?> UpdateTransactionAsync(int id, UpdateTransactionDto updateDto);
        Task<bool> DeleteTransactionAsync(int id);
    }
}
