namespace DanmaobTisax.Application.Plans;

public sealed class PlanDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> ModuleCodes { get; set; } = new List<string>();
}
