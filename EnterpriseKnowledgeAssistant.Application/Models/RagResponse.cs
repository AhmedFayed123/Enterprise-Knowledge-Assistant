using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class RagResponse
    {
        public string Answer { get; set; } = string.Empty;

        public List<SourceCitation> Sources { get; set; } = [];
    }
}
