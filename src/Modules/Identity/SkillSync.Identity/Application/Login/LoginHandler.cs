using Microsoft.AspNetCore.Authentication.OAuth;

using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Application.Common;
using SkillSync.Identity.Domain;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.Login;

internal sealed class LoginHandler(
    IIdentityUserService identityUserService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) : ICommandHandler<LoginCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var validationResult = LoginValidator.Validate(command);

        if (validationResult.IsFailure)
            return Result.Failure<AuthTokensResponse>(validationResult.Error);
                
        var userResult = await identityUserService.FindUserByEmailAsync(command.Email, cancellationToken);

        if (userResult.IsFailure)
        {
            if (userResult.Error == IdentityErrors.UserNotFound)
                return Result.Failure<AuthTokensResponse>(IdentityErrors.InvalidCredentials);

            return Result.Failure<AuthTokensResponse>(userResult.Error);
        }

        var userId = userResult.Value;

        var passwordResult = await identityUserService.CheckPasswordAsync(userId, command.Password, cancellationToken);

        if (passwordResult.IsFailure)
            return Result.Failure<AuthTokensResponse>(passwordResult.Error);

        if (!passwordResult.Value)
            return Result.Failure<AuthTokensResponse>(IdentityErrors.InvalidCredentials);

        var rolesResult = await identityUserService.GetUserRoleAsync(userId, cancellationToken);

        if (rolesResult.IsFailure)
            return Result.Failure<AuthTokensResponse>(rolesResult.Error);

        var accessTokenResult = tokenService.CreateAccessToken(userId, rolesResult.Value);

        var refreshToken = await refreshTokenService.CreateAsync(userId, cancellationToken);

        return Result.Success(new AuthTokensResponse(accessTokenResult, refreshToken));
    }
}
