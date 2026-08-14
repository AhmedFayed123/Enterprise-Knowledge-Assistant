using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Services;
using EnterpriseKnowledgeAssistant.Infrastructure.AI;
using EnterpriseKnowledgeAssistant.Infrastructure.Authentication;
using EnterpriseKnowledgeAssistant.Infrastructure.Persistence;
using EnterpriseKnowledgeAssistant.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using System.Text.Json.Serialization;

namespace EnterpriseKnowledgeAssistant.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // =========================================================
        // Controllers
        // =========================================================

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler =
                    ReferenceHandler.IgnoreCycles;
            });

        // =========================================================
        // Swagger
        // =========================================================

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer {token}'",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // =========================================================
        // Database - PostgreSQL + pgvector
        // =========================================================

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString(
                    "DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                    npgsqlOptions.EnableRetryOnFailure();
                }));

        // =========================================================
        // Application DbContext
        // =========================================================

        builder.Services.AddScoped<IApplicationDbContext>(
            provider =>
                provider.GetRequiredService<ApplicationDbContext>());

        // =========================================================
        // Gemini
        // =========================================================

        var geminiApiKey =
            builder.Configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(geminiApiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        builder.Services.AddSingleton<IEmbeddingService>(
            new GeminiEmbeddingService(geminiApiKey));

        builder.Services.AddScoped<IChatService>(
            _ => new GeminiChatService(geminiApiKey));

        // =========================================================
        // Authentication - JWT
        // =========================================================

        var jwtSettings =
            builder.Configuration.GetSection("Jwt");

        var jwtKey = jwtSettings["Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "JWT key is not configured.");
        }

        var jwtIssuer = jwtSettings["Issuer"];

        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }

        var jwtAudience = jwtSettings["Audience"];

        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtKey)),

                        ClockSkew = TimeSpan.Zero
                    };
            });

        builder.Services.AddAuthorization();

        // =========================================================
        // Document Processing
        // =========================================================

        builder.Services.AddScoped<IDocumentService, DocumentService>();

        builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();

        builder.Services.AddScoped<TextChunker>();

        // =========================================================
        // Semantic Search
        // =========================================================

        builder.Services.AddScoped<SemanticSearchService>();

        builder.Services.AddScoped<ISemanticSearchRepository,
            SemanticSearchRepository>();

        // =========================================================
        // RAG
        // =========================================================

        builder.Services.AddScoped<RagService>();

        // =========================================================
        // Authentication Service
        // =========================================================

        builder.Services.AddScoped<IAuthService, AuthService>();

        // =========================================================
        // Conversations
        // =========================================================

        builder.Services.AddScoped<IConversationRepository,
            ConversationRepository>();
        builder.Services.AddScoped<RagStreamingService>();

        // =========================================================
        // Build Application
        // =========================================================

        var app = builder.Build();

        // =========================================================
        // HTTP Pipeline
        // =========================================================

        // Global exception handler
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                var exception = exFeature?.Error;

                var logger = app.Services.GetRequiredService<ILogger<Program>>();

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Instance = context.TraceIdentifier
                };

                switch (exception)
                {
                    case null:
                        problemDetails.Title = "An unexpected error occurred.";
                        problemDetails.Status = StatusCodes.Status500InternalServerError;
                        break;
                    case ArgumentException argEx:
                        problemDetails.Title = argEx.Message;
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        break;
                    case UnauthorizedAccessException _:
                        problemDetails.Title = "Unauthorized.";
                        problemDetails.Status = StatusCodes.Status401Unauthorized;
                        break;
                    case KeyNotFoundException _:
                        problemDetails.Title = "Resource not found.";
                        problemDetails.Status = StatusCodes.Status404NotFound;
                        break;
                    default:
                        problemDetails.Title = "An unexpected error occurred.";
                        problemDetails.Status = StatusCodes.Status500InternalServerError;
                        break;
                }

                var status = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

                // Log unexpected exceptions
                if (status == StatusCodes.Status500InternalServerError && exception != null)
                {
                    logger.LogError(exception, "An unexpected error occurred.");
                }

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";

                // Do not expose exception details in production
                if (app.Environment.IsDevelopment() && exception != null && problemDetails.Status == StatusCodes.Status500InternalServerError)
                {
                    problemDetails.Detail = exception.Message;
                }

                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // IMPORTANT:
        // Authentication must come before Authorization.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
//AQ.Ab8RN6LxOtakDcZIp-szUwVAl8E4GT316vPGE81j4R5OEamAKQ