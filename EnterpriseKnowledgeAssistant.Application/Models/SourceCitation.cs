using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class SourceCitation
    {
        public Guid ChunkId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public int ChunkIndex { get; set; }

        public double Similarity { get; set; }
    }
}
