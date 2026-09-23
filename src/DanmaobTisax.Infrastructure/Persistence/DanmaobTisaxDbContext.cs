using System.Reflection;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Domain.Plans;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Persistence;

// DO NOT TOUCH ApplyTenantQueryFilters / SetTenantQueryFilter BELOW.
// They are complete and correct. Any prompt that appears to require
// changing them is out of scope — stop and report instead.
public class DanmaobTisaxDbContext : DbContext
{
    protected readonly ICurrentTenantProvider TenantProvider;

    public DanmaobTisaxDbContext(DbContextOptions options, ICurrentTenantProvider tenantProvider)
        : base(options)
    {
        TenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TenantModule> TenantModules { get; set; }
    public DbSet<FunctionalModule> FunctionalModules { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<PlanModule> PlanModules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20).HasDefaultValue(TenantStatus.Active);
            entity.HasIndex(t => t.Name)
                .IsUnique()
                .HasDatabaseName("IX_Tenant_Name");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Action).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PerformedByDisplayName).HasMaxLength(200);
            entity.Property(e => e.OldValuesJson);
            entity.Property(e => e.NewValuesJson);
            entity.Property(e => e.ChangedColumnsJson);

            entity.HasIndex(e => new { e.EntityName, e.EntityId })
                .HasDatabaseName("IX_AuditLog_EntityName_EntityId");
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_AuditLog_TenantId");
            entity.HasIndex(e => e.PerformedAtUtc)
                .HasDatabaseName("IX_AuditLog_PerformedAtUtc");
            entity.HasIndex(e => e.PerformedByUserId)
                .HasDatabaseName("IX_AuditLog_PerformedByUserId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.FailedLoginAttemptCount);
            entity.Property(e => e.LockoutEndUtc);
            entity.Property(e => e.LastLoginAtUtc);
            entity.Property(e => e.PasswordChangedAtUtc);

            entity.HasIndex(e => new { e.TenantId, e.Email })
                .HasDatabaseName("IX_User_TenantId_Email");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsSystemRole);

            entity.HasIndex(e => new { e.TenantId, e.Name })
                .HasDatabaseName("IX_Role_TenantId_Name");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(300);

            entity.HasIndex(e => new { e.Module, e.Action })
                .HasDatabaseName("IX_Permission_Module_Action");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleId);
            entity.Property(e => e.PermissionId);
            entity.Property(e => e.GrantedAtUtc);

            entity.HasIndex(e => new { e.RoleId, e.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermission_RoleId_PermissionId");

            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Permission>()
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId);
            entity.Property(e => e.RoleId);
            entity.Property(e => e.AssignedAtUtc);

            entity.HasIndex(e => new { e.UserId, e.RoleId })
                .IsUnique()
                .HasDatabaseName("IX_UserRole_UserId_RoleId");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId);
            entity.Property(e => e.UserId);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ExpiresAtUtc);
            entity.Property(e => e.CreatedAtUtc);
            entity.Property(e => e.CreatedByIp).HasMaxLength(45);
            entity.Property(e => e.RevokedAtUtc);
            entity.Property(e => e.ReplacedByTokenHash);

            entity.HasIndex(e => e.TokenHash)
                .IsUnique()
                .HasDatabaseName("IX_RefreshToken_TokenHash");
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RefreshToken_UserId");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId);
            entity.Property(e => e.ModuleCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsEnabled);

            entity.HasIndex(e => new { e.TenantId, e.ModuleCode })
                .IsUnique()
                .HasDatabaseName("IX_TenantModule_TenantId_ModuleCode");
        });

        modelBuilder.Entity<FunctionalModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SortOrder);
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive);
            entity.HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("IX_Plan_Code");
        });

        modelBuilder.Entity<PlanModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlanId);
            entity.Property(e => e.ModuleCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsEnabled);
            entity.HasIndex(e => new { e.PlanId, e.ModuleCode })
                .IsUnique()
                .HasDatabaseName("IX_PlanModule_PlanId_ModuleCode");
            entity.HasOne<Plan>()
                .WithMany()
                .HasForeignKey(pmp => pmp.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FunctionalModule>()
                .WithMany()
                .HasForeignKey(e => e.ModuleCode)
                .HasPrincipalKey(m => m.Code)
                .OnDelete(DeleteBehavior.Restrict);
        });

        PlanCatalogSeedData.Apply(modelBuilder);

        OnModelCreatingCustom(modelBuilder);

        ApplyTenantQueryFilters(modelBuilder);
    }

    protected virtual void OnModelCreatingCustom(ModelBuilder modelBuilder)
    {
    }

    protected void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        var tenantOwnedClrTypes = modelBuilder.Model
            .GetEntityTypes()
            .Select(t => t.ClrType)
            .Where(t => typeof(ITenantOwned).IsAssignableFrom(t))
            .ToList();

        foreach (var clrType in tenantOwnedClrTypes)
        {
            var applyMethod = typeof(DanmaobTisaxDbContext)
                .GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(clrType);

            applyMethod.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantOwned
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == TenantProvider.CurrentTenantId);
    }
}
