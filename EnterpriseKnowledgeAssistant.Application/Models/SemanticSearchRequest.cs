using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class SemanticSearchRequest
    {
        public string Query { get; set; } = string.Empty;

        public int TopK { get; set; } = 5;
    }
}
