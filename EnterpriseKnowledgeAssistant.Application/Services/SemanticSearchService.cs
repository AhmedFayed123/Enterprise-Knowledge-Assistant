using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Services
{
    public class SemanticSearchService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ISemanticSearchRepository _searchRepository;

        public SemanticSearchService(
            IEmbeddingService embeddingService,
            ISemanticSearchRepository searchRepository)
        {
            _embeddingService = embeddingService;
            _searchRepository = searchRepository;
        }

        public async Task<List<SemanticSearchResult>> SearchAsync(
            string query,
            int topK = 5,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException(
                    "Search query cannot be empty.",
                    nameof(query));

            if (topK <= 0)
                throw new ArgumentException(
                    "TopK must be greater than zero.",
                    nameof(topK));

            var queryEmbedding =
                await _embeddingService.GenerateQueryEmbeddingAsync(
                    query,
                    cancellationToken);

            return await _searchRepository.SearchAsync(
                queryEmbedding,
                topK,
                cancellationToken);
        }    
    }
}
