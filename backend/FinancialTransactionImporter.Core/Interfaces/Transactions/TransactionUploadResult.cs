using FinancialTransactionImporter.Core.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.Interfaces.Transactions
{
    public class TransactionUploadResult
    {
        public bool Success { get; set; }
        public int ProcessedRows { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
