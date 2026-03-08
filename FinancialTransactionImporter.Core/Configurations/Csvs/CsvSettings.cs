using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.Configurations.Csvs
{
    public class CsvSettings
    {
        public string Delimiter { get; set; } = ",";
        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public string[] ExpectedHeaders { get; set; } = { "TransactionTime", "Amount", "Description", "TransactionId" };
    }
}
