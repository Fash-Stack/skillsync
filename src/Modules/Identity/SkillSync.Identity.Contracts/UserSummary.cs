namespace SkillSync.Identity.Contracts;

public sealed record UserSummary(Guid Id, string Email, IReadOnlyCollection<string> Roles);
