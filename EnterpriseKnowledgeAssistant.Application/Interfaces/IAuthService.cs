
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);

        Task<string> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);
    }
}
