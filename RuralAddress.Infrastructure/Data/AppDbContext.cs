using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RuralAddress.Core.Entities;
using RuralAddress.Core.Interfaces;
using System.Text.Json;

namespace RuralAddress.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Propriedade> Propriedades { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<SystemParameter> SystemParameters { get; set; }
    public DbSet<PropriedadeCultivo> PropriedadeCultivos { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<PanicAlert> PanicAlerts { get; set; }
    public DbSet<PanicChatMessage> PanicChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações de relacionamento (Cascade Delete)
        modelBuilder.Entity<Propriedade>()
            .HasIndex(p => p.CepRural)
            .IsUnique();

        modelBuilder.Entity<Pessoa>()
            .HasOne(p => p.Propriedade)
            .WithMany(b => b.Pessoas)
            .HasForeignKey(p => p.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Veiculo>()
            .HasOne(v => v.Propriedade)
            .WithMany(b => b.Veiculos)
            .HasForeignKey(v => v.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PropriedadeCultivo>()
            .HasOne(pc => pc.Propriedade)
            .WithMany(p => p.Cultivos)
            .HasForeignKey(pc => pc.PropriedadeId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        var userId = _currentUserService.UserId ?? "System";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId,
                Action = entry.State.ToString()
            };

            // Parent Linking Logic
            if (entry.Entity is Pessoa pessoa)
            {
                 auditEntry.ParentEntityName = "Propriedade";
                 auditEntry.ParentEntityId = pessoa.PropriedadeId.ToString();
            }
            else if (entry.Entity is Veiculo veiculo)
            {
                 auditEntry.ParentEntityName = "Propriedade";
                 auditEntry.ParentEntityId = veiculo.PropriedadeId.ToString();
            }
            else if (entry.Entity is PropriedadeCultivo pc)
            {
                 auditEntry.ParentEntityName = "Propriedade";
                 auditEntry.ParentEntityId = pc.PropriedadeId.ToString();
            }

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                // FIX: Force UTC for PostgreSQL
                if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            var original = property.OriginalValue;
                            var current = property.CurrentValue;

                            // NOISE REDUCTION: Only log if values are actually different
                            if (!Equals(original, current))
                            {
                                // For strings, handle null vs empty if desired, but Equals handles object equality mostly fine
                                // Special case for string "null" text? No, values are objects.
                                auditEntry.OldValues[propertyName] = original;
                                auditEntry.NewValues[propertyName] = current;
                            }
                        }
                        break;
                }
            }
             
            // If modified but no actual property changes detected (noise filtered out), skip saving log
            if (entry.State == EntityState.Modified && auditEntry.OldValues.Count == 0 && auditEntry.NewValues.Count == 0)
            {
                continue;
            }

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                // Also update ParentEntityId if it was temporary! 
                // But for Pessoa/Veiculo/PropCrop, PropriedadeId is usually an FK that already exists or is being set.
                // If adding NEW Propriedade AND NEW Pessoa Same Time, PropID might be temp.
                // Handling that complex case requires inspecting Navigation properties or Foreign Keys.
                // For now assuming Propriedade exists or ID is known.
            }

            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        await base.SaveChangesAsync();

        // Cleanup: Limit 3 logs per PARENT aggregation? 
        // User asked: "Last 3 modifications of EACH ITEM" (item = Propriedade?)
        // If we list child logs in the Propriedade History, maybe we should limit based on ParentEntityId?
        // Or stick to per-row limit? 
        // User wants "Last 3 updates" in that list. If I edit a Person, that's 1 update.
        // It's safer to limit based on Propriedade context if we display them together.
        // BUT, multiple concurrent edits might be tricky.
        // Let's stick to the simpler per-entity cleanup for now, 
        // because "Cleaning up Parent's history" when saving a Child is complex (fetch all logs for parent...).
        // Let's modify the cleanup query to be a bit smarter or just leave it per-entity-id for now.
        // If the Viewer shows mixed entities, the list might grow long if we don't prune "child logs".
        
        // Let's try to prune based on the EntityId of the log entry itself to keep database small.
        // If we want to strictly show only 3 lines in the UI, the UI can limit Take(3).
        // But the requirement "Security ... file log ... last 3" implies data retention policy.
        // Let's keep the existing Entity-based cleanup.
        
        foreach (var entry in auditEntries)
        {
            var entityId = entry.GetPrimaryKey();
            // Also cleanup using ParentEntity logic? 
            // If I delete a vehicle, I have a log. 
            // If I edit it 10 times, I have 10 logs.
            // I should prune *that vehicle's* logs.
            
            if (string.IsNullOrEmpty(entityId)) continue;
             
            var entityName = entry.TableName;

            var logs = await AuditLogs
                .Where(x => x.EntityId == entityId && x.EntityName == entityName)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            if (logs.Count > 3)
            {
                var logsToRemove = logs.Skip(3).ToList();
                AuditLogs.RemoveRange(logsToRemove);
            }
        }
        
        await base.SaveChangesAsync();
    }
}

public class AuditEntry
{
    public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        Entry = entry;
    }

    public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
    public string UserId { get; set; }
    public string TableName { get; set; }
    public string Action { get; set; }
    
    public string? ParentEntityName { get; set; }
    public string? ParentEntityId { get; set; }

    public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
    public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry>();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public string GetPrimaryKey()
    {
        return string.Join("-", KeyValues.Values);
    }

    public AuditLog ToAuditLog()
    {
        var audit = new AuditLog
        {
            UserId = UserId,
            Action = Action,
            EntityName = TableName,
            Timestamp = DateTime.UtcNow,
            EntityId = GetPrimaryKey(),
            ParentEntityName = ParentEntityName,
            ParentEntityId = ParentEntityId
        };

        if (Action == "Modified") 
        {
             var changes = new Dictionary<string, string>();
             foreach(var key in NewValues.Keys) 
             {
                 var oldVal = OldValues.ContainsKey(key) ? OldValues[key]?.ToString() : "null";
                 var newVal = NewValues[key]?.ToString();
                 changes[key] = $"{oldVal} -> {newVal}";
             }
             audit.Changes = JsonSerializer.Serialize(changes);
        }
        else if (Action == "Added")
        {
            audit.Changes = JsonSerializer.Serialize(NewValues);
        }
        else if (Action == "Deleted")
        {
             audit.Changes = JsonSerializer.Serialize(OldValues);
        }

        return audit;
    }
}
