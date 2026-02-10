using Microsoft.EntityFrameworkCore;
using ScioApp.Models;

namespace ScioApp.Data;

/// <summary>
/// Databázový kontext pro Scio aplikaci
/// Všechny tabulky mají prefix Scio_ pro vyhnutí se konfliktům s existujícími tabulkami
/// </summary>
public class ScioDbContext : DbContext
{
    public ScioDbContext(DbContextOptions<ScioDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ProgressLog> ProgressLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ========================================
        // KONFIGURACE TABULEK S PREFIXEM Scio_
        // ========================================
        
        modelBuilder.Entity<User>().ToTable("Scio_Users");
        modelBuilder.Entity<Group>().ToTable("Scio_Groups");
        modelBuilder.Entity<Student>().ToTable("Scio_Students");
        modelBuilder.Entity<Message>().ToTable("Scio_Messages");
        modelBuilder.Entity<ProgressLog>().ToTable("Scio_ProgressLogs");
        
        // ========================================
        // USER - Učitelský účet
        // ========================================
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Login)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Login)
                .IsUnique();
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255);
            
            entity.HasIndex(e => e.GoogleId)
                .IsUnique()
                .HasFilter("[GoogleId] IS NOT NULL"); // Partial index - pouze non-null hodnoty
            
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Relationships
            entity.HasMany(u => u.Groups)
                .WithOne(g => g.Teacher)
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ========================================
        // GROUP - Skupina s cílem
        // ========================================
        
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.GoalDescription)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(e => e.GoalType)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.Property(e => e.InviteCode)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.InviteCode)
                .IsUnique();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
            
            // Relationships
            entity.HasMany(g => g.Students)
                .WithOne(s => s.Group)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(g => g.Messages)
                .WithOne(m => m.Group)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ========================================
        // STUDENT - Student v rámci skupiny
        // ========================================
        
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nickname)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(255);
            
            // Composite unique index - DeviceId + Nickname musí být unique per skupina
            entity.HasIndex(e => new { e.GroupId, e.DeviceId, e.Nickname })
                .IsUnique();
            
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.LastActivityAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(StudentStatus.Active);
            
            // Relationships
            entity.HasMany(s => s.Messages)
                .WithOne(m => m.Student)
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Nemazat zprávy při smazání studenta
            
            entity.HasOne(s => s.ProgressLog)
                .WithOne(p => p.Student)
                .HasForeignKey<ProgressLog>(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ========================================
        // MESSAGE - Chatová zpráva
        // ========================================
        
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(5000);
            
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.IsSystemMessage)
                .HasDefaultValue(false);
            
            entity.Property(e => e.IsProgressContribution)
                .HasDefaultValue(false);
            
            // Index pro rychlé načítání zpráv ve skupině
            entity.HasIndex(e => new { e.GroupId, e.Timestamp });
        });
        
        // ========================================
        // PROGRESSLOG - Log pokroku studenta
        // ========================================
        
        modelBuilder.Entity<ProgressLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CurrentValue)
                .HasDefaultValue(0);
            
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false);
            
            entity.Property(e => e.LastUpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique index - jeden ProgressLog per student
            entity.HasIndex(e => e.StudentId)
                .IsUnique();
        });
    }
}
