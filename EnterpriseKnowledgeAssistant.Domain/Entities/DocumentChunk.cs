using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Pgvector.EntityFrameworkCore;
namespace EnterpriseKnowledgeAssistant.Domain.Entities
{
    public class DocumentChunk
    {
        public Guid Id { get; set; }

        public Guid DocumentId { get; set; }

        public string Content { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }

        public int PageNumber { get; set; }

        public Pgvector.Vector? Embedding { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Document Document { get; set; } = null!;
    }
}
