using Elsa.Extensions;
using Elsa.Workflows;
using Procurement.Application.Abstractions;

namespace Procurement.Application.Workflows;

public class BudgetAvailabilityActivity : Activity
{
    private readonly IBudgetAvailabilityChecker _budgetAvailabilityChecker;

    public BudgetAvailabilityActivity(IBudgetAvailabilityChecker budgetAvailabilityChecker)
    {
        _budgetAvailabilityChecker = budgetAvailabilityChecker;
    }

    public string AmountVariable { get; set; } = "Amount";

    public string ResultVariable { get; set; } = "BudgetAvailable";

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var amount = context.GetVariable<decimal?>(AmountVariable) ?? 0;
        var isAvailable = await _budgetAvailabilityChecker.IsBudgetAvailableAsync(amount, context.CancellationToken);
        context.SetVariable(ResultVariable, isAvailable);
    }
}
