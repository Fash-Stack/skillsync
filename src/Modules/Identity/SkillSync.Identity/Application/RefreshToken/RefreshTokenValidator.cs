using SkillSync.Identity.Domain;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.RefreshToken;

internal static class RefreshTokenValidator
{
    public static Result Validate(RefreshTokenCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Failure(IdentityErrors.InvalidRefreshToken);
        }

        return Result.Success();
    }
}
