using EnterpriseKnowledgeAssistant.Domain.Entities;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Document> Documents { get; }

    void AddDocument(Document document);

    Task<List<Document>> GetDocumentsWithChunksAsync(
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}