using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Api.Messaging;

internal sealed class IntegrationEventDispatcher(
    IServiceProvider serviceProvider)
    : IIntegrationEventDispatcher
{
    public async Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        cancellationToken.ThrowIfCancellationRequested();

        var handlerType = typeof(IIntegrationEventConsumer<>)
            .MakeGenericType(integrationEvent.GetType());

        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handleMethod = handlerType.GetMethod(nameof
                (IIntegrationEventConsumer<IIntegrationEvent>.Handle));

            if (handleMethod is null)
                throw new InvalidOperationException($"Handler method was not found for {handlerType.Name}");

            var task = (Task?)handleMethod.Invoke(handler, [integrationEvent, cancellationToken]);

            if (task is null)
                throw new InvalidOperationException($"Handler {handlerType.Name} returned no task.");

            await task;      
        }
    }
}
