using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseKnowledgeAssistant.Domain.Entities;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<object> UploadAsync(IFormFile file);
        Task<IEnumerable<Document>> GetAllAsync();
    }
}
