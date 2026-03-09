using FinancialTransactionImporter.Core.Configurations.Csvs;
using FinancialTransactionImporter.Core.Interfaces.Csvs;
using FinancialTransactionImporter.Core.Library;
using FinancialTransactionImporter.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Infrastructure.Services
{
    public class CsvParserService : ICsvParserService
    {
        private readonly CsvSettings _csvSettings;

        public CsvParserService(IOptions<CsvSettings> csvSettings)
        {
            _csvSettings = csvSettings.Value;
        }

        public async Task<(List<Transaction> ValidTransactions, List<ValidationError> Errors)> ParseCsvAsync(IFormFile file)
        {
            var validTransactions = new List<Transaction>();
            var errors = new List<ValidationError>();
            var lineNumber = 0;

            using var stream = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            string? line;

            // Read and validate headers
            line = await stream.ReadLineAsync();
            lineNumber++;

            if (string.IsNullOrEmpty(line))
            {
                errors.Add(new ValidationError { RowNumber = 0, Column = "Header", Error = "File is empty" });
                return (validTransactions, errors);
            }

            var headers = line.Split(_csvSettings.Delimiter);
            var headerErrors = ValidateHeaders(headers);
            if (headerErrors.Any())
            {
                errors.AddRange(headerErrors);
                return (validTransactions, errors);
            }

            // Process data rows
            while ((line = await stream.ReadLineAsync()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var columns = line.Split(_csvSettings.Delimiter);

                if (columns.Length != _csvSettings.ExpectedHeaders.Length)
                {
                    errors.Add(new ValidationError
                    {
                        RowNumber = lineNumber,
                        Column = "All",
                        Error = $"Expected {_csvSettings.ExpectedHeaders.Length} columns, found {columns.Length}"
                    });
                    continue;
                }

                var (transaction, rowErrors) = ValidateRow(columns, lineNumber);

                if (rowErrors.Any())
                {
                    errors.AddRange(rowErrors);
                }
                else if (transaction != null)
                {
                    validTransactions.Add(transaction);
                }
            }

            return (validTransactions, errors);
        }

        public List<ValidationError> ValidateHeaders(string[] headers)
        {
            var errors = new List<ValidationError>();

            for (int i = 0; i < _csvSettings.ExpectedHeaders.Length; i++)
            {
                if (i >= headers.Length || !headers[i].Trim().Equals(_csvSettings.ExpectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new ValidationError
                    {
                        RowNumber = 1,
                        Column = _csvSettings.ExpectedHeaders[i],
                        Error = $"Expected header '{_csvSettings.ExpectedHeaders[i]}' at position {i + 1}"
                    });
                }
            }

            return errors;
        }

        public (Transaction?, List<ValidationError>) ValidateRow(string[] columns, int rowNumber)
        {
            var errors = new List<ValidationError>();
            var transaction = new Transaction();

            // Validate TransactionTime
            if (!DateTime.TryParseExact(columns[0].Trim(), _csvSettings.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionTime))
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    Column = "TransactionTime",
                    Error = $"Invalid date format. Expected format: {_csvSettings.DateFormat}"
                });
            }
            else
            {
                transaction.TransactionTime = transactionTime;
            }

            // Validate Amount
            if (!decimal.TryParse(columns[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    Column = "Amount",
                    Error = "Invalid amount format"
                });
            }
            else if (decimal.Round(amount, 2) != amount)
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    Column = "Amount",
                    Error = "Amount must have exactly 2 decimal places"
                });
            }
            else if (amount <= 0)
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    Column = "Amount",
                    Error = "Amount must be greater than 0"
                });
            }
            else
            {
                transaction.Amount = amount;
            }

            // Validate Description
            var description = columns[2].Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    Column = "Description",
                    Error = "Description cannot be empty"
                });
            }
            else
            {
                transaction.Description = description;
            }

            // Validate TransactionId
            var transactionId = columns[3].Trim();
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    Column = "TransactionId",
                    Error = "TransactionId cannot be empty"
                });
            }
            else
            {
                transaction.TransactionId = transactionId;
            }

            transaction.CreatedAt = DateTime.UtcNow;

            return errors.Any() ? (null, errors) : (transaction, errors);
        }
    }
}
