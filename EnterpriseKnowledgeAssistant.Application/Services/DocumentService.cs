using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnterpriseKnowledgeAssistant.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IApplicationDbContext _context;
        private readonly IPdfTextExtractor _pdfTextExtractor;

        public DocumentService(IApplicationDbContext context, IPdfTextExtractor pdfTextExtractor)
        {
            _context = context;
            _pdfTextExtractor = pdfTextExtractor;
        }

        public async Task<object> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Please upload a file.");

            if (!Path.GetExtension(file.FileName)
                .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only PDF files are supported.");
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = _pdfTextExtractor.ExtractText(filePath);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                Status = "Processed"
            };

            // Persist
            _context.AddDocument(document);
            await _context.SaveChangesAsync();

            return new
            {
                document.Id,
                document.FileName,
                document.Status,
                TextLength = extractedText.Length
            };
        }

        public Task<IEnumerable<Document>> GetAllAsync()
        {
            var list = System.Linq.Enumerable.ToList(_context.Documents);
            return Task.FromResult<IEnumerable<Document>>(list);
        }
    }
}
