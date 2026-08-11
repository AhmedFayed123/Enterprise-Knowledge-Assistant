using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public string Status { get; set; } = "Pending";
        public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    }
}
