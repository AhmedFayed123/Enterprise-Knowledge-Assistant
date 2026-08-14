using EnterpriseKnowledgeAssistant.Application.Models;
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
    // New streaming orchestration service that exposes streaming events for SSE.
    public class RagStreamingService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ISemanticSearchRepository _searchRepository;
        private readonly IChatService _chatService;
        private readonly IConversationRepository _conversationRepository;

        public RagStreamingService(
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

        public async IAsyncEnumerable<StreamEvent> StreamInConversationAsync(
            Guid conversationId,
            string question,
            Guid userId,
            int topK = 5,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Validate ownership and get/create conversation
            Conversation? conversation = null;

            if (conversationId != Guid.Empty)
            {
                conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
                if (conversation == null || conversation.UserId != userId)
                    throw new KeyNotFoundException("Conversation not found.");
            }

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Title = question.Length > 100 ? question[..100] : question,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                await _conversationRepository.AddAsync(conversation, cancellationToken);
                await _conversationRepository.SaveChangesAsync(cancellationToken);
            }

            // Save user message before generation
            var userMessage = new Domain.Entities.ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                Role = "user",
                Content = question,
                CreatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddMessageAsync(userMessage, cancellationToken);
            await _conversationRepository.SaveChangesAsync(cancellationToken);

            // Build history (last N messages)
            var history = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new EnterpriseKnowledgeAssistant.Application.Models.ChatMessage { Role = m.Role, Content = m.Content })
                .ToList();

            const int MaxHistory = 10;
            if (history.Count > MaxHistory)
                history = history.Skip(history.Count - MaxHistory).ToList();

            // Retrieve relevant chunks
            var queryEmbedding = await _embeddingService.GenerateQueryEmbeddingAsync(question, cancellationToken);
            var chunks = await _searchRepository.SearchAsync(queryEmbedding, topK, cancellationToken);

            if (chunks.Count == 0)
            {
                yield return new StreamEvent { Type = "error", Content = "I could not find enough information in the provided documents." };
                yield return new StreamEvent { Type = "done" };
                yield break;
            }

            var context = string.Join(
                "\n\n--- SOURCE ---\n\n",
                chunks.Select(chunk =>
                    $"Document: {chunk.FileName}\nPage: {chunk.PageNumber}\nContent:\n{chunk.Content}"));

            // Stream tokens from chat service
            var sb = new StringBuilder();

            await foreach (var token in _chatService.StreamAnswerAsync(question, context, history, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.Append(token);
                yield return new StreamEvent { Type = "token", Content = token };
            }

            var assistantFull = sb.ToString();

            // Save assistant message
            var assistantMessage = new Domain.Entities.ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = assistantFull,
                CreatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddMessageAsync(assistantMessage, cancellationToken);
            await _conversationRepository.SaveChangesAsync(cancellationToken);

            // Return sources
            var sources = chunks.Select(chunk => new SourceCitation
            {
                ChunkId = chunk.ChunkId,
                FileName = chunk.FileName,
                PageNumber = chunk.PageNumber,
                ChunkIndex = chunk.ChunkIndex,
                Similarity = chunk.Similarity
            }).ToList();

            yield return new StreamEvent { Type = "sources", Sources = sources };
            yield return new StreamEvent { Type = "done" };
        }
    }
}
