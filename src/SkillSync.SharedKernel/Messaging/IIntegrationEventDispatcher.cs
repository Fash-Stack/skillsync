namespace SkillSync.SharedKernel.Messaging;

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
