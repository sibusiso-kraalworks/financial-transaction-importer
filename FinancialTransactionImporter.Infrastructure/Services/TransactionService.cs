using FinancialTransactionImporter.Core.DTOs.Transactions;
using FinancialTransactionImporter.Core.Interfaces;
using FinancialTransactionImporter.Core.Interfaces.Transactions;
using FinancialTransactionImporter.Core.Library;
using FinancialTransactionImporter.Core.Models;
using FinancialTransactionImporter.Infrastructure.DbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using FinancialTransactionImporter.Core.Interfaces.Csvs;

namespace FinancialTransactionImporter.Infrastructure.Services
{
    /// <summary>
    /// Provides transaction management services including upload, retrieval, update, and deletion of transactions.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICsvParserService _csvParser;
        private readonly ILogger<TransactionService> _logger;

        /// <summary>
        /// Initializes a new instance of TransactionService with database context, CSV parser, and logger.
        /// </summary>
        public TransactionService(
            ApplicationDbContext context,
            ICsvParserService csvParser,
            ILogger<TransactionService> logger)
        {
            _context = context;
            _csvParser = csvParser;
            _logger = logger;
        }

        /// <summary>
        /// Uploads transactions from a CSV file, validates them, checks for duplicates, and saves valid transactions to the database.
        /// </summary>
        /// <param name="file">The uploaded CSV file containing transactions.</param>
        /// <returns>Result of the upload operation including success status, errors, and processed row count.</returns>
        public async Task<TransactionUploadResult> UploadTransactionsAsync(IFormFile file)
        {
            var result = new TransactionUploadResult();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.Message = "No file uploaded";
                    return result;
                }

                // Parse and validate CSV
                var (transactions, errors) = await _csvParser.ParseCsvAsync(file);

                if (errors.Any())
                {
                    result.Errors = errors;
                    result.Message = $"Validation failed with {errors.Count} errors";
                    return result;
                }

                if (!transactions.Any())
                {
                    result.Message = "No valid transactions found in file";
                    return result;
                }

                // Check for duplicate TransactionIds
                var transactionIds = transactions.Select(t => t.TransactionId).ToList();
                var existingIds = await _context.Transactions
                    .Where(t => transactionIds.Contains(t.TransactionId))
                    .Select(t => t.TransactionId)
                    .ToListAsync();

                if (existingIds.Any())
                {
                    foreach (var id in existingIds)
                    {
                        errors.Add(new ValidationError
                        {
                            RowNumber = 0,
                            Column = "TransactionId",
                            Error = $"Duplicate TransactionId: {id} already exists in database"
                        });
                    }
                    result.Errors = errors;
                    result.Message = "Duplicate transaction IDs found";
                    return result;
                }

                // Save to database
                await _context.Transactions.AddRangeAsync(transactions);
                await _context.SaveChangesAsync();

                result.Success = true;
                result.ProcessedRows = transactions.Count;
                result.Message = $"Successfully uploaded {transactions.Count} transactions";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading transactions");
                result.Message = $"Error uploading file: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Retrieves a paged list of transactions ordered by transaction time.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>Paged result containing transactions and pagination info.</returns>
        public async Task<PagedResult<Transaction>> GetTransactionsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Transactions.OrderByDescending(t => t.TransactionTime);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Transaction>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Retrieves a transaction by its unique identifier.
        /// </summary>
        /// <param name="id">The transaction ID.</param>
        /// <returns>The transaction if found; otherwise, null.</returns>
        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        /// <summary>
        /// Updates an existing transaction with new values.
        /// </summary>
        /// <param name="id">The transaction ID to update.</param>
        /// <param name="updateDto">The DTO containing updated transaction data.</param>
        /// <returns>The updated transaction if found; otherwise, null.</returns>
        public async Task<Transaction?> UpdateTransactionAsync(int id, UpdateTransactionDto updateDto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return null;

            transaction.TransactionTime = updateDto.TransactionTime;
            transaction.Amount = updateDto.Amount;
            transaction.Description = updateDto.Description;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transaction;
        }

        /// <summary>
        /// Deletes a transaction by its unique identifier.
        /// </summary>
        /// <param name="id">The transaction ID to delete.</param>
        /// <returns>True if the transaction was deleted; otherwise, false.</returns>
        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
