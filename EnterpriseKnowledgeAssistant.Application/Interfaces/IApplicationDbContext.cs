using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseKnowledgeAssistant.Domain.Entities;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        IQueryable<Document> Documents { get; }

        void AddDocument(Document document);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
