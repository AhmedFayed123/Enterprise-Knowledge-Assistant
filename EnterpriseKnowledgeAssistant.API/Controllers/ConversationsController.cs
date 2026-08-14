using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Models;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseKnowledgeAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly EnterpriseKnowledgeAssistant.Application.Services.RagService _ragService;
        private readonly EnterpriseKnowledgeAssistant.Application.Services.RagStreamingService _ragStreamingService;

        public ConversationsController(IConversationRepository conversationRepository,
            EnterpriseKnowledgeAssistant.Application.Services.RagService ragService,
            EnterpriseKnowledgeAssistant.Application.Services.RagStreamingService ragStreamingService)
        {
            _conversationRepository = conversationRepository;
            _ragService = ragService;
            _ragStreamingService = ragStreamingService;
        }

        [HttpPost("{conversationId:guid}/messages/stream")]
        public async Task StreamMessageStream(Guid conversationId, [FromBody] CreateMessageRequest request)
        {
            // Streaming endpoint uses HttpContext.RequestAborted for cancellation
            var cancellationToken = HttpContext.RequestAborted;

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                Response.StatusCode = 400;
                await Response.WriteAsJsonAsync(new { message = "Content is required." }, cancellationToken);
                return;
            }

            if (request.Content.Length > 2000)
            {
                Response.StatusCode = 400;
                await Response.WriteAsJsonAsync(new { message = "Content is too long (max 2000 characters)." }, cancellationToken);
                return;
            }

            var userId = GetUserIdFromClaims();

            // Ownership check
            var ownerId = await _conversationRepository.GetOwnerIdAsync(conversationId, cancellationToken);
            if (!ownerId.HasValue || ownerId.Value != userId)
            {
                Response.StatusCode = 404;
                return;
            }

            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";

            await foreach (var evt in _ragStreamingService.StreamInConversationAsync(conversationId, request.Content, userId, 5, cancellationToken))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(evt);
                var sse = $"data: {json}\n\n";
                await Response.WriteAsync(sse, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("Invalid user id claim.");
            }

            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateConversationRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Title is required." });

            var userId = GetUserIdFromClaims();

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                CreatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation, cancellationToken);
            await _conversationRepository.SaveChangesAsync(cancellationToken);

            var dto = new ConversationDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { conversationId = dto.Id }, dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyConversations(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();

            var conversations = await _conversationRepository.GetUserConversationsAsync(userId, cancellationToken);

            var result = conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{conversationId:guid}")]
        public async Task<IActionResult> GetById(Guid conversationId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();

            var conversation = await _conversationRepository.GetUserConversationByIdAsync(userId, conversationId, cancellationToken);

            if (conversation == null)
                return NotFound();

            var dto = new ConversationDetailsDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt,
                Messages = conversation.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ChatMessageDto
                    {
                        Id = m.Id,
                        Role = m.Role,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt
                    }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("{conversationId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();

            var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

            if (conversation == null || conversation.UserId != userId)
                return NotFound();

            var messages = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToList();

            return Ok(messages);
        }

        [HttpPost("{conversationId:guid}/messages")]
        public async Task<IActionResult> PostMessage(Guid conversationId, [FromBody] CreateMessageRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Content is required." });

            if (request.Content.Length > 2000)
                return BadRequest(new { message = "Content is too long (max 2000 characters)." });

            var userId = GetUserIdFromClaims();

            // Check ownership without loading messages
            var ownerId = await _conversationRepository.GetOwnerIdAsync(conversationId, cancellationToken);

            if (!ownerId.HasValue || ownerId.Value != userId)
                return NotFound();

            try
            {
                var result = await _ragService.AskInConversationAsync(conversationId, request.Content, userId, 5, cancellationToken);

                return Ok(new
                {
                    conversationId = result.ConversationId,
                    question = request.Content,
                    answer = result.Answer,
                    sources = result.Sources.Select(s => new
                    {
                        chunkId = s.ChunkId,
                        fileName = s.FileName,
                        pageNumber = s.PageNumber,
                        similarity = s.Similarity
                    })
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{conversationId:guid}")]
        public async Task<IActionResult> Delete(Guid conversationId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();

            if (conversationId == Guid.Empty)
                return BadRequest(new { message = "Invalid conversation id." });

            var deleted = await _conversationRepository.DeleteConversationAsync(userId, conversationId, cancellationToken);

            if (!deleted)
                return NotFound();

            await _conversationRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
