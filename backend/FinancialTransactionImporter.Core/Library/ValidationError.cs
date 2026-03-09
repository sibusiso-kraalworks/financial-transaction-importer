using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.Library
{
    public class ValidationError
    {
        public int RowNumber { get; set; }
        public string Column { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
