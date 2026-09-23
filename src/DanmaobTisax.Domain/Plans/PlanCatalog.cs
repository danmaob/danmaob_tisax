namespace DanmaobTisax.Domain.Plans;

public static class PlanCatalog
{
    public const string FreeCode = "FREE";
    public const string BasicCode = "BASIC";
    public const string PremiumCode = "PREMIUM";

    public static readonly Guid FreePlanId = new Guid("7a0b3c10-0000-4000-8000-000000000001");
    public static readonly Guid BasicPlanId = new Guid("7a0b3c10-0000-4000-8000-000000000002");
    public static readonly Guid PremiumPlanId = new Guid("7a0b3c10-0000-4000-8000-000000000003");
}
