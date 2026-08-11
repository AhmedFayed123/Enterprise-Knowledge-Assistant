using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
// removed System.Reflection.Metadata to avoid 'Document' type ambiguity
using System.Text;
using System.Threading.Tasks;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using EnterpriseKnowledgeAssistant.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace EnterpriseKnowledgeAssistant.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Domain.Entities.Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

        IQueryable<Domain.Entities.Document> IApplicationDbContext.Documents => Documents;

        public void AddDocument(Domain.Entities.Document document)
        {
            Documents.Add(document);
        }
    }
}
