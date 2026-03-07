using Microsoft.Extensions.DependencyInjection;
using YousifAccounting.Application.Security;

namespace YousifAccounting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<PasswordValidator>();

        return services;
    }
}
