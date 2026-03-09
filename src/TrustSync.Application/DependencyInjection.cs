using Microsoft.Extensions.DependencyInjection;
using TrustSync.Application.Security;

namespace TrustSync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<PasswordValidator>();

        return services;
    }
}
