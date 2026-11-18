using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Application.Common;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.RefreshToken;

internal sealed class RefreshTokenHandler(
    IRefreshTokenService refreshTokenService,
    IIdentityUserService identityUserService,
    ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> Handle(
        RefreshTokenCommand refreshTokenCommand,
        CancellationToken cancellationToken)
    {
        var rotationResult = await refreshTokenService.RotateAsync(refreshTokenCommand.RefreshToken, cancellationToken);

        if (rotationResult.IsFailure)
        {
            return Result.Failure<AuthTokensResponse>(rotationResult.Error);
        }

        var rotatedToken = rotationResult.Value;

        var rolesResult = await identityUserService.GetUserRoleAsync(rotatedToken.UserId, cancellationToken);

        if (rolesResult.IsFailure)
        {
            return Result.Failure<AuthTokensResponse>(rolesResult.Error);
        }

        var accessToken = tokenService.CreateAccessToken(rotatedToken.UserId, rolesResult.Value);

        return Result.Success(new AuthTokensResponse(accessToken, rotatedToken.RawToken));
    }
}
