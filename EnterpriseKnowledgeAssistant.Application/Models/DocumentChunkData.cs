using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class DocumentChunkData
    {
        public string Content { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }

        public int PageNumber { get; set; }
    }
}
