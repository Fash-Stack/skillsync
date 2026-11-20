using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SkillSync.Identity.Contracts.Events;
using SkillSync.Identity.Infrastructure.Persistence;
using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Identity.Infrastructure.Outbox;

internal sealed partial class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var dispatcher = scope.ServiceProvider
            .GetRequiredService<IIntegrationEventDispatcher>();

        var messages = await dbContext.OutboxMessages
            .Where(messages => messages.ProcessedOnUtc == null)
            .OrderBy(messages => messages.OccuredAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (message.Type != typeof(UserRegisteredIntegrationEvent).FullName)
                    continue;

                var integrationEvent = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(message.Content)
                    ?? throw new InvalidOperationException("Could not deserialize user registration event.");

                await dispatcher.DispatchAsync(integrationEvent, cancellationToken);

                message.MarkAsProcessed(DateTimeOffset.UtcNow);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                OutboxProcessorLogs.OutboxMessageFailed(logger, exception, message.Id);
                message.MarkAsFailed(exception.Message);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
