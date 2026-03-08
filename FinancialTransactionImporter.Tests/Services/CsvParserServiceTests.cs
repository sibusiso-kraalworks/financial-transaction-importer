using FinancialTransactionImporter.Core.Configurations.Csvs;
using FinancialTransactionImporter.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FinancialTransactionImporter.Tests.Services
{
    /// <summary>
    /// Unit tests for CsvParserService to validate CSV parsing and error handling.
    /// </summary>
    public class CsvParserServiceTests
    {
        private readonly CsvSettings _csvSettings;
        private readonly Mock<IOptions<CsvSettings>> _mockOptions;
        private readonly CsvParserService _parser;

        /// <summary>
        /// Initializes test dependencies and CsvParserService instance.
        /// </summary>
        public CsvParserServiceTests()
        {
            _csvSettings = new CsvSettings
            {
                Delimiter = ",",
                DateFormat = "yyyy-MM-dd HH:mm:ss",
                ExpectedHeaders = new[] { "TransactionTime", "Amount", "Description", "TransactionId" }
            };

            _mockOptions = new Mock<IOptions<CsvSettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_csvSettings);
            _parser = new CsvParserService(_mockOptions.Object);
        }

        /// <summary>
        /// Tests parsing a valid CSV file returns correct transactions and no errors.
        /// </summary>
        [Fact]
        public async Task ParseCsvAsync_ValidCsv_ReturnsTransactions()
        {
            // Arrange
            var csvContent = "TransactionTime,Amount,Description,TransactionId\n" +
                            "2024-01-15 10:30:00,123.45,Test Transaction 1,TRX001\n" +
                            "2024-01-15 11:45:00,678.90,Test Transaction 2,TRX002";

            var file = CreateMockFile(csvContent);

            // Act
            var (transactions, errors) = await _parser.ParseCsvAsync(file);

            // Assert
            Assert.Empty(errors);
            Assert.Equal(2, transactions.Count);
            Assert.Equal("TRX001", transactions[0].TransactionId);
            Assert.Equal(123.45m, transactions[0].Amount);
        }

        /// <summary>
        /// Tests parsing a CSV file with invalid date format returns a validation error.
        /// </summary>
        [Fact]
        public async Task ParseCsvAsync_InvalidDateFormat_ReturnsError()
        {
            // Arrange
            var csvContent = "TransactionTime,Amount,Description,TransactionId\n" +
                            "2024/01/15 10:30,123.45,Test Transaction,TRX001";

            var file = CreateMockFile(csvContent);

            // Act
            var (transactions, errors) = await _parser.ParseCsvAsync(file);

            // Assert
            Assert.Empty(transactions);
            Assert.Single(errors);
            Assert.Contains("Invalid date format", errors[0].Error);
        }

        /// <summary>
        /// Tests parsing a CSV file with amount having more than two decimals returns a validation error.
        /// </summary>
        [Fact]
        public async Task ParseCsvAsync_AmountWithMoreThanTwoDecimals_ReturnsError()
        {
            // Arrange
            var csvContent = "TransactionTime,Amount,Description,TransactionId\n" +
                            "2024-01-15 10:30:00,123.456,Test Transaction,TRX001";

            var file = CreateMockFile(csvContent);

            // Act
            var (transactions, errors) = await _parser.ParseCsvAsync(file);

            // Assert
            Assert.Empty(transactions);
            Assert.Single(errors);
            Assert.Contains("exactly 2 decimal places", errors[0].Error);
        }

        /// <summary>
        /// Tests parsing a CSV file with empty description returns a validation error.
        /// </summary>
        [Fact]
        public async Task ParseCsvAsync_EmptyDescription_ReturnsError()
        {
            // Arrange
            var csvContent = "TransactionTime,Amount,Description,TransactionId\n" +
                            "2024-01-15 10:30:00,123.45,,TRX001";

            var file = CreateMockFile(csvContent);

            // Act
            var (transactions, errors) = await _parser.ParseCsvAsync(file);

            // Assert
            Assert.Empty(transactions);
            Assert.Single(errors);
            Assert.Contains("Description cannot be empty", errors[0].Error);
        }

        /// <summary>
        /// Tests parsing an empty CSV file returns a validation error.
        /// </summary>
        [Fact]
        public async Task ParseCsvAsync_EmptyFile_ReturnsError()
        {
            // Arrange
            var csvContent = "";
            var file = CreateMockFile(csvContent);

            // Act
            var (transactions, errors) = await _parser.ParseCsvAsync(file);

            // Assert
            Assert.Empty(transactions);
            Assert.Single(errors);
            Assert.Equal("File is empty", errors[0].Error);
        }

        /// <summary>
        /// Tests parsing a CSV file with incorrect headers returns header validation errors.
        /// </summary>
        [Fact]
        public async Task ParseCsvAsync_WrongHeaders_ReturnsError()
        {
            // Arrange
            var csvContent = "WrongHeader1,WrongHeader2,WrongHeader3,WrongHeader4\n" +
                            "2024-01-15 10:30:00,123.45,Test Transaction,TRX001";

            var file = CreateMockFile(csvContent);

            // Act
            var (transactions, errors) = await _parser.ParseCsvAsync(file);

            // Assert
            Assert.Empty(transactions);
            Assert.Equal(4, errors.Count);
        }

        /// <summary>
        /// Creates a mock IFormFile from a string content for testing.
        /// </summary>
        private IFormFile CreateMockFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();

            file.Setup(f => f.OpenReadStream()).Returns(stream);
            file.Setup(f => f.FileName).Returns("test.csv");
            file.Setup(f => f.Length).Returns(stream.Length);

            return file.Object;
        }
    }
}
