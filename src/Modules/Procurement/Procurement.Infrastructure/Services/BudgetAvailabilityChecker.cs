using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Procurement.Application.Abstractions;

namespace Procurement.Infrastructure.Services;

public class BudgetAvailabilityChecker : IBudgetAvailabilityChecker
{
    private readonly ILogger<BudgetAvailabilityChecker> _logger;
    private readonly BudgetOptions _options;

    public BudgetAvailabilityChecker(IOptions<BudgetOptions> options, ILogger<BudgetAvailabilityChecker> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<bool> IsBudgetAvailableAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        var available = amount <= _options.MaxAmount;
        _logger.LogInformation("Budget check for {Amount} against limit {Limit}: {Result}", amount, _options.MaxAmount, available);
        return Task.FromResult(available);
    }
}

public class BudgetOptions
{
    public decimal MaxAmount { get; set; } = 1_000_000M;
}
