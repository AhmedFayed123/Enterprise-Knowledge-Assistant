using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseKnowledgeAssistant.Application.Models;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IChatService
    {
        Task<string> GenerateAnswerAsync(
    string question,
    string context,
    IReadOnlyList<ChatMessage>? history = null,
    CancellationToken cancellationToken = default);

        IAsyncEnumerable<string> StreamAnswerAsync(
            string question,
            string context,
            IReadOnlyList<ChatMessage>? history = null,
            CancellationToken cancellationToken = default);
    }
}
