using EnterpriseKnowledgeAssistant.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdAsync(
      Guid conversationId,
      CancellationToken cancellationToken = default);

        Task<List<Conversation>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<Guid?> GetOwnerIdAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default);

        Task<List<Conversation>> GetUserConversationsAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<Conversation?> GetUserConversationByIdAsync(
            Guid userId,
            Guid conversationId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Conversation conversation,
            CancellationToken cancellationToken = default);

        Task AddMessageAsync(
            ChatMessage message,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteConversationAsync(
            Guid userId,
            Guid conversationId,
            CancellationToken cancellationToken = default);
    }
}
