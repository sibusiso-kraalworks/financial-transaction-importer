using FinancialTransactionImporter.Core.Library;
using FinancialTransactionImporter.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.Interfaces.Csvs
{
    public interface ICsvParserService
    {
        /// <summary>
        /// Parses a CSV file and validates its contents
        /// </summary>
        /// <param name="file">The uploaded CSV file</param>
        /// <returns>A tuple containing valid transactions and validation errors</returns>
        Task<(List<Transaction> ValidTransactions, List<ValidationError> Errors)> ParseCsvAsync(IFormFile file);

        /// <summary>
        /// Validates the CSV headers against expected headers
        /// </summary>
        /// <param name="headers">The headers from the CSV file</param>
        /// <returns>List of validation errors, if any</returns>
        List<ValidationError> ValidateHeaders(string[] headers);

        /// <summary>
        /// Validates a single row of CSV data
        /// </summary>
        /// <param name="columns">The columns from the CSV row</param>
        /// <param name="rowNumber">The row number being validated</param>
        /// <returns>A tuple containing a valid transaction and validation errors</returns>
        (Transaction? Transaction, List<ValidationError> Errors) ValidateRow(string[] columns, int rowNumber);
    }
}
