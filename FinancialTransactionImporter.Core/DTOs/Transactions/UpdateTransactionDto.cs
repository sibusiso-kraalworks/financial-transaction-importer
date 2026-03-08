using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.DTOs.Transactions
{
    public class UpdateTransactionDto
    {
        [Required]
        public DateTime TransactionTime { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Description { get; set; } = string.Empty;
    }
}
