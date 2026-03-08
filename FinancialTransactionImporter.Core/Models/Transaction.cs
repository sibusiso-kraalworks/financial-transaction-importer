using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
