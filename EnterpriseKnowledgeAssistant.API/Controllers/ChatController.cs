using EnterpriseKnowledgeAssistant.Application.Models;
using EnterpriseKnowledgeAssistant.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
     [FromBody] ChatRequest request,
     CancellationToken cancellationToken)
        {
            var result = await _ragService.AskAsync(
                request.Question,
                request.TopK,
                cancellationToken);

            return Ok(new
            {
                question = request.Question,
                answer = result.Answer,
                sources = result.Sources
            });
        }
    }
}
