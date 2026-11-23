namespace SkillSync.Identity.Contracts;

public interface IIdentityApi
{
    Task<UserSummary?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
}
