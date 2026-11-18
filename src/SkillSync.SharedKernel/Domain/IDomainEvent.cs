namespace SkillSync.SharedKernel.Domain;

public interface IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccuredOnUtc { get; }
}
