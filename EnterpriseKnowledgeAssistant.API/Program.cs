using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Services;
using EnterpriseKnowledgeAssistant.Infrastructure.Persistence;
using EnterpriseKnowledgeAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using EnterpriseKnowledgeAssistant.Infrastructure.AI;
namespace EnterpriseKnowledgeAssistant.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Prevent JSON serializer from throwing on object cycles caused by EF navigation properties
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseNpgsql(
           builder.Configuration.GetConnectionString("DefaultConnection"),
           npgsqlOptions =>
           {
               npgsqlOptions.UseVector();
           }));
        var geminiApiKey = builder.Configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(geminiApiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        builder.Services.AddSingleton<IEmbeddingService>(
            new GeminiEmbeddingService(geminiApiKey));
        // register application services and infra implementations
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();
        // Map the application-facing Db context interface to the EF DbContext implementation
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<TextChunker>();
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

//AQ.Ab8RN6LxOtakDcZIp-szUwVAl8E4GT316vPGE81j4R5OEamAKQ