using EnterpriseKnowledgeAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseKnowledgeAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbeddingsController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public EmbeddingsController(
        IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [HttpPost]
    public async Task<IActionResult> Generate(
        [FromBody] string text,
        CancellationToken cancellationToken)
    {
        var embedding = await _embeddingService
            .GenerateEmbeddingAsync(text, cancellationToken);

        return Ok(new
        {
            dimensions = embedding.Length,
            firstValues = embedding.Take(5)
        });
    }
}