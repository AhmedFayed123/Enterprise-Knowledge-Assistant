using EnterpriseKnowledgeAssistant.Domain.Entities;
using System.Linq;
using EnterpriseKnowledgeAssistant.Domain.Entities;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        IQueryable<Document> Documents { get; }

        IQueryable<User> Users { get; }

        void AddUser(User user);

        void AddDocument(Document document);

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task<List<Document>> GetDocumentsWithChunksAsync(
            CancellationToken cancellationToken = default);
    }
}