using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Persistence
{
    public class SemanticSearchRepository : ISemanticSearchRepository
    {
        private readonly ApplicationDbContext _context;

        public SemanticSearchRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<SemanticSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, CancellationToken cancellationToken = default)
        {
            var vector = new Vector(queryEmbedding);

            return await _context.DocumentChunks
                .AsNoTracking()
                .Where(x => x.Embedding != null)
                .OrderBy(x => x.Embedding!.CosineDistance(vector))
                .Take(topK)
                .Select(x => new SemanticSearchResult
                {
                    ChunkId = x.Id,
                    DocumentId = x.DocumentId,
                    FileName = x.Document!.FileName,
                    Content = x.Content,
                    PageNumber = x.PageNumber,
                    ChunkIndex = x.ChunkIndex,
                    Similarity = 1 - x.Embedding!.CosineDistance(vector)
                })
                .ToListAsync(cancellationToken);
        
        }
    }
}
