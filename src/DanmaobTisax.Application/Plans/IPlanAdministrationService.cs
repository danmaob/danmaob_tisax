using System.Collections.Generic;

namespace DanmaobTisax.Application.Plans;

public interface IPlanAdministrationService
{
    /// <summary>
    /// Returns the functional module catalog ordered by SortOrder.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that resolves to the ordered list of functional modules.</returns>
    Task<IReadOnlyList<FunctionalModuleDto>> GetModuleCatalogAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns all plans ordered by Code, each with its enabled module codes.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that resolves to the list of all plans.</returns>
    Task<IReadOnlyList<PlanDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns the plan, or null when it does not exist.
    /// </summary>
    /// <param name="planId">The identifier of the plan to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that resolves to the plan or null if not found.</returns>
    Task<PlanDto?> GetByIdAsync(Guid planId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an active plan with no modules. Outcomes: Succeeded, InvalidCode, InvalidName, CodeAlreadyExists.
    /// </summary>
    /// <param name="code">The code of the new plan.</param>
    /// <param name="name">The name of the new plan.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that resolves to the operation result.</returns>
    Task<PlanOperationResult> CreateAsync(string code, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Renames a plan. Outcomes: Succeeded, NotFound, InvalidName.
    /// </summary>
    /// <param name="planId">The identifier of the plan to rename.</param>
    /// <param name="name">The new name for the plan.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that resolves to the operation result.</returns>
    Task<PlanOperationResult> RenameAsync(Guid planId, string name, CancellationToken cancellationToken);
}
