using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    IQueryable<Document> IApplicationDbContext.Documents
        => Documents;

    public void AddDocument(Document document)
    {
        Documents.Add(document);
    }
    public async Task<List<Document>> GetDocumentsWithChunksAsync(
    CancellationToken cancellationToken = default)
    {
        return await Documents
            .Include(d => d.Chunks)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}