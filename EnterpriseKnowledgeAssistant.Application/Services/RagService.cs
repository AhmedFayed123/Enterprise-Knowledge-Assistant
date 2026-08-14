using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Models;
using EnterpriseKnowledgeAssistant.Domain.Entities;
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
        private readonly IConversationRepository _conversationRepository;

        public RagService(
            IEmbeddingService embeddingService,
            ISemanticSearchRepository searchRepository,
            IChatService chatService,
            IConversationRepository conversationRepository)
        {
            _embeddingService = embeddingService;
            _searchRepository = searchRepository;
            _chatService = chatService;
            _conversationRepository = conversationRepository;
        }

        public async Task<RagResponse> AskAsync(
           string question,
           int topK = 5,
           List<Models.ChatMessage>? history = null,
           CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(
                    "Question cannot be empty.",
                    nameof(question));

            history ??= new List<Models.ChatMessage>();

            // 1. Generate embedding for the current question
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

            // 3. Build retrieved context
            var context = string.Join(
                "\n\n--- SOURCE ---\n\n",
                chunks.Select(chunk =>
                    $"Document: {chunk.FileName}\n" +
                    $"Page: {chunk.PageNumber}\n" +
                    $"Content:\n{chunk.Content}"));

            // 4. Prepare conversation history (limit to recent messages)
            const int MaxHistory = 10;
            var recentHistory = history;
            if (recentHistory.Count > MaxHistory)
            {
                recentHistory = recentHistory.Skip(recentHistory.Count - MaxHistory).ToList();
            }

            // 5. Generate answer using chat service, passing history separately
            var answer = await _chatService.GenerateAnswerAsync(
                question,
                context,
                recentHistory,
                cancellationToken);





            // 7. Build citations
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
        public async Task<RagResponse> AskInConversationAsync(
    Guid? conversationId,
    string question,
    Guid userId,
    int topK = 5,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(
                    "Question cannot be empty.",
                    nameof(question));

            Conversation? conversation = null;

            // Existing conversation
            if (conversationId.HasValue)
            {
                conversation =
                    await _conversationRepository.GetByIdAsync(
                        conversationId.Value,
                        cancellationToken);

                if (conversation == null)
                    throw new KeyNotFoundException(
                        "Conversation not found.");
                // Ensure the conversation belongs to the requesting user
                if (conversation.UserId != userId)
                    throw new KeyNotFoundException(
                        "Conversation not found.");
            }

            // Create new conversation
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Title = question.Length > 100
                        ? question[..100]
                        : question,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                await _conversationRepository.AddAsync(
                    conversation,
                    cancellationToken);

                await _conversationRepository.SaveChangesAsync(
                    cancellationToken);
            }

            // Build conversation history
            var history = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new Models.ChatMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })
                .ToList();

            // Retrieve relevant document chunks
            var queryEmbedding =
                await _embeddingService.GenerateQueryEmbeddingAsync(
                    question,
                    cancellationToken);

            var chunks = await _searchRepository.SearchAsync(
                queryEmbedding,
                topK,
                cancellationToken);

            if (chunks.Count == 0)
            {
                return new RagResponse
                {
                    ConversationId = conversation.Id,
                    Answer =
        "I could not find enough information in the provided documents.",
                    Sources = []
                };
            }

            var context = string.Join(
                "\n\n--- SOURCE ---\n\n",
                chunks.Select(chunk =>
                    $"Document: {chunk.FileName}\n" +
                    $"Page: {chunk.PageNumber}\n" +
                    $"Content:\n{chunk.Content}"));

            // Limit conversation history to recent messages
            const int MaxHistory = 10;
            var recentHistory = history;
            if (recentHistory.Count > MaxHistory)
            {
                recentHistory = recentHistory.Skip(recentHistory.Count - MaxHistory).ToList();
            }

            var answer = await _chatService.GenerateAnswerAsync(
                question,
                context,
                recentHistory,
                cancellationToken);

            // Save user message
            await _conversationRepository.AddMessageAsync(
                new Domain.Entities.ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    Role = "user",
                    Content = question,
                    CreatedAt = DateTime.UtcNow
                },
                cancellationToken);

            // Save assistant message
            await _conversationRepository.AddMessageAsync(
                new Domain.Entities.ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    Role = "assistant",
                    Content = answer,
                    CreatedAt = DateTime.UtcNow
                },
                cancellationToken);

            await _conversationRepository.SaveChangesAsync(
                cancellationToken);

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
                ConversationId = conversation.Id,
                Answer = answer,
                Sources = sources
            };
        }
    }
}
