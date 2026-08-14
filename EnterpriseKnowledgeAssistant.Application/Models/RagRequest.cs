using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class RagRequest
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
        public List<ChatMessage> History { get; set; } = [];
    
    }
}
