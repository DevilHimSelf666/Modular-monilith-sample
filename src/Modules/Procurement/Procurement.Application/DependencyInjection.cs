using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Procurement.Application.Workflows;

namespace Procurement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProcurementApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        services.AddScoped<BudgetAvailabilityActivity>();
        return services;
    }
}
