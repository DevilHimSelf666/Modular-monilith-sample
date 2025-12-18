namespace Procurement.Application.Abstractions;

public interface IBudgetAvailabilityChecker
{
    Task<bool> IsBudgetAvailableAsync(decimal amount, CancellationToken cancellationToken = default);
}
