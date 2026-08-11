using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Pgvector;

namespace EnterpriseKnowledgeAssistant.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly TextChunker _textChunker;
    private readonly IEmbeddingService _embeddingService;

    public DocumentService(
        IApplicationDbContext context,
        IPdfTextExtractor pdfTextExtractor,
        TextChunker textChunker,
        IEmbeddingService embeddingService)
    {
        _context = context;
        _pdfTextExtractor = pdfTextExtractor;
        _textChunker = textChunker;
        _embeddingService = embeddingService;
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

        await using (var stream = new FileStream(
            filePath,
            FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Extract PDF pages
        var pages = _pdfTextExtractor.ExtractPages(filePath);

        // Split pages into chunks
        var chunks = _textChunker.Split(pages);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow,
            Status = "Processing"
        };

        // Generate embeddings and create chunk entities
        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingService
                .GenerateEmbeddingAsync(chunk.Content);

            document.Chunks.Add(new DocumentChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Content = chunk.Content,
                ChunkIndex = chunk.ChunkIndex,
                PageNumber = chunk.PageNumber,
                Embedding = new Vector(embedding)
            });
        }

        document.Status = "Processed";

        // Save document and chunks
        _context.AddDocument(document);

        await _context.SaveChangesAsync();

        return new
        {
            document.Id,
            document.FileName,
            document.Status,
            Pages = pages.Count,
            Chunks = chunks.Count
        };
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.GetDocumentsWithChunksAsync();
    }
}