using System.Text.Json;
using SkillSync.SharedKernel.Messaging;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

using SkillSync.Identity.Contracts.Events;
using SkillSync.Identity.Infrastructure.Persistence;

namespace SkillSync.Identity.Infrastructure.Outbox;

internal sealed partial class OutboxProcessor(
    IdentityDbContext dbContext,
    IIntegrationEventDispatcher dispatcher,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
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
                {
                    throw new InvalidOperationException($"Unsupported outbox message type: {message.Type}");
                }
                    
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
