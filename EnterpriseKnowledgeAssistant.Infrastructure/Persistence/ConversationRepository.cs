using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Persistence
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConversationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Conversation?> GetByIdAsync(
         Guid conversationId,
         CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(
                    c => c.Id == conversationId,
                    cancellationToken);
        }

        public async Task<List<Conversation>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Where(c => c.UserId == userId)
                // Do not include messages for this call (lightweight list)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Conversation?> GetUserConversationByIdAsync(
            Guid userId,
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Where(c => c.Id == conversationId && c.UserId == userId)
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Guid?> GetOwnerIdAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Conversations
                .Where(c => c.Id == conversationId)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            Conversation conversation,
            CancellationToken cancellationToken = default)
        {
            await _context.Conversations.AddAsync(
                conversation,
                cancellationToken);
        }

        public async Task AddMessageAsync(
            ChatMessage message,
            CancellationToken cancellationToken = default)
        {
            await _context.ChatMessages.AddAsync(
                message,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId,
                    cancellationToken);

            if (conversation != null)
            {
                _context.Conversations.Remove(conversation);
            }
        }

        public async Task<bool> DeleteConversationAsync(
            Guid userId,
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId,
                    cancellationToken);

            if (conversation == null)
                return false;

            _context.Conversations.Remove(conversation);
            return true;
        }
    }
}
