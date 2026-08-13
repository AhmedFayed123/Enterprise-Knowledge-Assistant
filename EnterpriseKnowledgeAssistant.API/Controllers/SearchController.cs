using EnterpriseKnowledgeAssistant.Application.Models;
using EnterpriseKnowledgeAssistant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseKnowledgeAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SemanticSearchService _searchService;

        public SearchController(SemanticSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
        public async Task<IActionResult> Search(
            [FromBody] SemanticSearchRequest request,
            CancellationToken cancellationToken)
        {
            var results = await _searchService.SearchAsync(
                request.Query,
                request.TopK,
                cancellationToken);

            return Ok(results);
        }    
    }
}
