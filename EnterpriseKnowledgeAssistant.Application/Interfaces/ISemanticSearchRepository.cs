using EnterpriseKnowledgeAssistant.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface ISemanticSearchRepository
    {
        Task<List<SemanticSearchResult>> SearchAsync(
    float[] queryEmbedding,
    int topK = 5,
    CancellationToken cancellationToken = default);
    }
}
