using FlowDesigner.Application.Interfaces.Repositories;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Infrastructure.Persistence;
using FlowDesigner.Infrastructure.Auth;
using FlowDesigner.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesigner.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=flowdesigner.db";

        services.Configure<AuthOptions>(configuration.GetSection("GitHubOAuth"));
        services.AddHttpContextAccessor();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
