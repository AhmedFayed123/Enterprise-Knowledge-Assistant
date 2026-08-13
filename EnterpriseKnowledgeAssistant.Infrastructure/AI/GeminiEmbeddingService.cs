using EnterpriseKnowledgeAssistant.Application.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using System.Text;
using System.Security.Cryptography;
using EnterpriseKnowledgeAssistant.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Infrastructure.AI;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly Client _client;

    private const string Model = "gemini-embedding-001";
    private const int EmbeddingDimensions = 1536;

    public GeminiEmbeddingService(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException(
                "Gemini API key is required.",
                nameof(apiKey));

        _client = new Client(apiKey: apiKey);
    }

    // NOTE: A simple deterministic fallback embedding generator is provided here
    // so the project builds and runs even without calling the external Gemini API.
    // Replace this implementation with real API calls using _client when ready.
    private Task<float[]> GenerateEmbeddingAsync(
        string text,
        string embeddingType,
        CancellationToken cancellationToken = default)
    {
        // Deterministic pseudo-random embedding based on SHA256 of the text
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty));
        var seed = BitConverter.ToInt32(hash, 0);
        var rnd = new Random(seed);

        var embedding = new float[EmbeddingDimensions];
        for (int i = 0; i < EmbeddingDimensions; i++)
        {
            // values in range [-1, 1)
            embedding[i] = (float)(rnd.NextDouble() * 2.0 - 1.0);
        }

        return Task.FromResult(embedding);
    }

    public Task<float[]> GenerateDocumentEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        return GenerateEmbeddingAsync(
            text,
            "RETRIEVAL_DOCUMENT",
            cancellationToken);
    }

    public Task<float[]> GenerateQueryEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        return GenerateEmbeddingAsync(
            text,
            "RETRIEVAL_QUERY",
            cancellationToken);
    }
}