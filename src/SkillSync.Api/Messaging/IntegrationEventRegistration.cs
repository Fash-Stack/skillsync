using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Api.Messaging;

internal static class IntegrationEventRegistration
{
    internal static IServiceCollection AddIntegrationEventMessaging(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        return services;
    }
}
