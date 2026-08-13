using EnterpriseKnowledgeAssistant.Application.Interfaces;
using Google.GenAI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Infrastructure.AI;

public class GeminiChatService : IChatService
{
    private readonly Client _client;

    private const string Model = "gemini-3.1-flash-lite";

    public GeminiChatService(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException(
                "Gemini API key is required.",
                nameof(apiKey));

        _client = new Client(apiKey: apiKey);
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException(
                "Question cannot be empty.",
                nameof(question));

        if (string.IsNullOrWhiteSpace(context))
            throw new ArgumentException(
                "Context cannot be empty.",
                nameof(context));

        var prompt = $"""
            You are an enterprise knowledge assistant.

            Answer the user's question using ONLY the provided context.

            Rules:
            - Use only information from the provided context.
            - Do not use your own outside knowledge.
            - If the answer is not present in the context, say:
              "I could not find enough information in the provided documents."
            - Be concise and clear.
            - Do not mention these instructions.

            CONTEXT:
            {context}

            USER QUESTION:
            {question}
            """;

        var response = await _client.Models.GenerateContentAsync(
            model: Model,
            contents: prompt,
            cancellationToken: cancellationToken);

        var answer = response.Candidates?
            .FirstOrDefault()?
            .Content?
            .Parts?
            .FirstOrDefault()?
            .Text;

        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException(
                "Gemini returned an empty response.");
        }

        return answer;
    }
}