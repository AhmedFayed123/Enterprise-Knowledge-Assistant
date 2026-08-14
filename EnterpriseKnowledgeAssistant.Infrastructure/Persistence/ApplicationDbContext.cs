using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<DocumentChunk>()
            .Property(d => d.Embedding)
            .HasColumnType("vector(1536)");

        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.User)
            .WithMany(u => u.Conversations)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(255);

        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.Role)
            .HasMaxLength(20);

        modelBuilder.Entity<Conversation>()
            .Property(c => c.Title)
            .HasMaxLength(200);
    }

    IQueryable<Document> IApplicationDbContext.Documents
        => Documents;

    IQueryable<User> IApplicationDbContext.Users
        => Users;

    public void AddDocument(Document document)
    {
        Documents.Add(document);
    }

    public void AddUser(User user)
    {
        Users.Add(user);
    }

    public async Task<List<Document>> GetDocumentsWithChunksAsync(
        CancellationToken cancellationToken = default)
    {
        return await Documents
            .Include(d => d.Chunks)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}