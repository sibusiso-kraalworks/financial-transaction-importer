using FinancialTransactionImporter.Core.DTOs.Transactions;
using FinancialTransactionImporter.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactionImporter.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionService transactionService,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { Message = "No file uploaded" });

                var result = await _transactionService.UploadTransactionsAsync(file);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        result.Message,
                        result.Errors,
                        result.ProcessedRows
                    });
                }

                return Ok(new
                {
                    result.Message,
                    result.ProcessedRows
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing upload");
                return StatusCode(500, new { Message = "An error occurred while processing the file" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _transactionService.GetTransactionsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return StatusCode(500, new { Message = "An error occurred while retrieving transactions" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                    return NotFound(new { Message = $"Transaction with ID {id} not found" });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while retrieving the transaction" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var transaction = await _transactionService.UpdateTransactionAsync(id, updateDto);
                if (transaction == null)
                    return NotFound(new { Message = $"Transaction with ID {id} not found" });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the transaction" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var deleted = await _transactionService.DeleteTransactionAsync(id);
                if (!deleted)
                    return NotFound(new { Message = $"Transaction with ID {id} not found" });

                return Ok(new { Message = "Transaction deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the transaction" });
            }
        }
    }
}
