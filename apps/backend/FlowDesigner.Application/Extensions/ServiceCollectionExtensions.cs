using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesigner.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
