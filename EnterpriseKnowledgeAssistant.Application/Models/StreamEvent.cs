using System;
using System.Collections.Generic;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class StreamEvent
    {
        public string Type { get; set; } = string.Empty; // token | sources | done | error

        public string? Content { get; set; }

        public List<SourceCitation>? Sources { get; set; }
    }
}
