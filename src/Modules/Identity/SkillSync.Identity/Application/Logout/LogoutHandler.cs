using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Domain;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.Logout;

internal sealed class LogoutHandler(IRefreshTokenService refreshTokenServise)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Failure(IdentityErrors.InvalidRefreshToken);
        }

        await refreshTokenServise.RevokeAsync(command.RefreshToken, cancellationToken);

        return Result.Success();
    }
}
