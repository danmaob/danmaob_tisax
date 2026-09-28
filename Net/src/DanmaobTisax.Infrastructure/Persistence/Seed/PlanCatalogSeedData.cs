using Microsoft.EntityFrameworkCore;
using DanmaobTisax.Domain.Plans;

namespace DanmaobTisax.Infrastructure.Persistence.Seed;

public static class PlanCatalogSeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FunctionalModule>().HasData(
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000001"), Code = FunctionalModuleCodes.Organization, SortOrder = 1 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000002"), Code = FunctionalModuleCodes.Scope, SortOrder = 2 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000003"), Code = FunctionalModuleCodes.Catalog, SortOrder = 3 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000004"), Code = FunctionalModuleCodes.GapAnalysis, SortOrder = 4 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000005"), Code = FunctionalModuleCodes.Risk, SortOrder = 5 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000006"), Code = FunctionalModuleCodes.Documents, SortOrder = 6 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000007"), Code = FunctionalModuleCodes.Evidence, SortOrder = 7 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000008"), Code = FunctionalModuleCodes.Capa, SortOrder = 8 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000009"), Code = FunctionalModuleCodes.Audits, SortOrder = 9 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000010"), Code = FunctionalModuleCodes.Readiness, SortOrder = 10 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000011"), Code = FunctionalModuleCodes.Dashboard, SortOrder = 11 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000012"), Code = FunctionalModuleCodes.Reports, SortOrder = 12 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000013"), Code = FunctionalModuleCodes.ThirdParties, SortOrder = 13 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000014"), Code = FunctionalModuleCodes.Assets, SortOrder = 14 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000015"), Code = FunctionalModuleCodes.PhysicalPrototype, SortOrder = 15 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000016"), Code = FunctionalModuleCodes.UsersAccess, SortOrder = 16 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000017"), Code = FunctionalModuleCodes.Notifications, SortOrder = 17 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000018"), Code = FunctionalModuleCodes.LabelLifecycle, SortOrder = 18 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000019"), Code = FunctionalModuleCodes.KnowledgeBase, SortOrder = 19 },
            new { Id = new Guid("7a0b3c20-0000-4000-8000-000000000020"), Code = FunctionalModuleCodes.Administration, SortOrder = 20 }
        );

        modelBuilder.Entity<Plan>().HasData(
            new { Id = PlanCatalog.FreePlanId, Code = PlanCatalog.FreeCode, Name = "Free", IsActive = true },
            new { Id = PlanCatalog.BasicPlanId, Code = PlanCatalog.BasicCode, Name = "Básico", IsActive = true },
            new { Id = PlanCatalog.PremiumPlanId, Code = PlanCatalog.PremiumCode, Name = "Premium", IsActive = true }
        );
    }
}
