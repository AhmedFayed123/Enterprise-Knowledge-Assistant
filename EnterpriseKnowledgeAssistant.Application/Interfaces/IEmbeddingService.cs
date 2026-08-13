using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateDocumentEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default);

        Task<float[]> GenerateQueryEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default);
    }
}
