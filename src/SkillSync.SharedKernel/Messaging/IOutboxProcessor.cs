namespace SkillSync.SharedKernel.Messaging;

public interface IOutboxProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}
