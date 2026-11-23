using Microsoft.AspNetCore.Identity;

using SkillSync.Identity.Contracts;
using SkillSync.Identity.Infrastructure.Identity;

namespace SkillSync.Identity.Infrastructure.PublicApi;

internal sealed class IdentityApi(
    UserManager<ApplicationUser> userManager)
    : IIdentityApi
{
    public async Task<UserSummary?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return null;

        var roles = await userManager.GetRolesAsync(user);

        return new UserSummary(
            user.Id,
            user.Email!,
            (IReadOnlyCollection<string>)roles);
    }
}
