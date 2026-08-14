using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class ConversationChatRequest
    {
        public Guid? ConversationId { get; set; }

        public string Question { get; set; } = string.Empty;

        public int TopK { get; set; } = 5;
    }
}
