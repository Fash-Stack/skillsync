namespace SkillSync.SharedKernel.Messaging;

public interface IIntegrationEventConsumer<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken cancellationToken);
}
