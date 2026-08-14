using System;
using System.Collections.Generic;

namespace EnterpriseKnowledgeAssistant.Application.Models
{
    public class CreateConversationRequest
    {
        public string Title { get; set; } = string.Empty;
    }

    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }

    public class CreateMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
