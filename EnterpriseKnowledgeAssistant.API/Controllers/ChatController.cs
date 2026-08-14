using EnterpriseKnowledgeAssistant.Application.Models;
using EnterpriseKnowledgeAssistant.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseKnowledgeAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController: ControllerBase
    {
        private readonly RagService _ragService;

        public ChatController(RagService ragService)
        {
            _ragService = ragService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask(
            [FromBody] RagRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _ragService.AskAsync(
                request.Question,
                request.TopK,
                request.History,
                cancellationToken);

            return Ok(new
            {
                question = request.Question,
                answer = result.Answer,
                sources = result.Sources
            });
        }
        [HttpPost("conversation")]
        [Authorize]
        public async Task<IActionResult> Chat(
            [FromBody] ConversationChatRequest request,
            CancellationToken cancellationToken)
        {
            // Get authenticated user id from claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            try
            {
                var result = await _ragService.AskInConversationAsync(
                    request.ConversationId,
                    request.Question,
                    userId,
                    request.TopK,
                    cancellationToken);

                return Ok(new
                {
                    conversationId = result.ConversationId,
                    question = request.Question,
                    answer = result.Answer,
                    sources = result.Sources
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
