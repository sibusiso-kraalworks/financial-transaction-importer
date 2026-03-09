using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialTransactionImporter.Core.Interfaces.Authentications
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string email, string password);
        Task<AuthResult> LoginAsync(string email, string password);
    }
}
