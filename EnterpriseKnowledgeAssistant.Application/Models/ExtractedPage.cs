using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class ExtractedPage
    {
        public int PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
