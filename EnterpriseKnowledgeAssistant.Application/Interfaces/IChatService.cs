using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IChatService
    {
        Task<string> GenerateAnswerAsync(
    string question,
    string context,
    CancellationToken cancellationToken = default);
    }
}
