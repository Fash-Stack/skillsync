namespace SkillSync.Identity.Contracts.Events;

public sealed record UserRegisteredIntegrationEvent(Guid UserId, string Email, DateTimeOffset OccuredOnUtc);