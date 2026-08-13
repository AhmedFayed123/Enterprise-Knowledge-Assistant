using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Services
{
    public class RagService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ISemanticSearchRepository _searchRepository;
        private readonly IChatService _chatService;

        public RagService(
            IEmbeddingService embeddingService,
            ISemanticSearchRepository searchRepository,
            IChatService chatService)
        {
            _embeddingService = embeddingService;
            _searchRepository = searchRepository;
            _chatService = chatService;
        }

        public async Task<RagResponse> AskAsync(
            string question,
            int topK = 5,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(
                    "Question cannot be empty.",
                    nameof(question));

            // 1. Generate query embedding
            var queryEmbedding =
                await _embeddingService.GenerateQueryEmbeddingAsync(
                    question,
                    cancellationToken);

            // 2. Retrieve relevant chunks
            var chunks = await _searchRepository.SearchAsync(
                queryEmbedding,
                topK,
                cancellationToken);

            if (chunks.Count == 0)
            {
                return new RagResponse
                {
                    Answer =
                        "I could not find enough information in the provided documents."
                };
            }

            // 3. Build context
            var context = string.Join(
                "\n\n--- SOURCE ---\n\n",
                chunks.Select(chunk =>
                    $"Document: {chunk.FileName}\n" +
                    $"Page: {chunk.PageNumber}\n" +
                    $"Content:\n{chunk.Content}"));

            // 4. Generate answer
            var answer =
                await _chatService.GenerateAnswerAsync(
                    question,
                    context,
                    cancellationToken);

            // 5. Build citations
            var sources = chunks
                .Select(chunk => new SourceCitation
                {
                    ChunkId = chunk.ChunkId,
                    FileName = chunk.FileName,
                    PageNumber = chunk.PageNumber,
                    ChunkIndex = chunk.ChunkIndex,
                    Similarity = chunk.Similarity
                })
                .ToList();

            return new RagResponse
            {
                Answer = answer,
                Sources = sources
            };
        }
    }
}
