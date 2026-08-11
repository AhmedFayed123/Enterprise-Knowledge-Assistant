using EnterpriseKnowledgeAssistant.Application.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;

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

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));

        var config = new EmbedContentConfig
        {
            TaskType = "RETRIEVAL_DOCUMENT",
            OutputDimensionality = EmbeddingDimensions
        };

        var response = await _client.Models.EmbedContentAsync(
            model: Model,
            contents: text,
            config: config,
            cancellationToken: cancellationToken);

        var values = response.Embeddings?
            .FirstOrDefault()?
            .Values;

        if (values == null || values.Count == 0)
        {
            throw new InvalidOperationException(
                "Gemini returned an empty embedding.");
        }

        return values.Select(x => (float)x).ToArray();
    }
}