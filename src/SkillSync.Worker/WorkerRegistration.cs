using SkillSync.Api.Messaging;
using SkillSync.Worker.Outbox;

namespace SkillSync.Worker;

public static class WorkerRegistration
{
    public static IServiceCollection AddSkillSyncWorker(
        this IServiceCollection services)
    {
        services.AddIntegrationEventMessaging();
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }
}