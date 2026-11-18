using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SkillSync.Identity.Application.Common;
using SkillSync.Identity.Application.RefreshToken;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Endpoints;
    
internal static class RefreshTokenEndpoint
{
    internal static RouteGroupBuilder MapRefreshTokenEndpoint(
        this RouteGroupBuilder group,
        IEndpointFilter resultFilter)
    {
        group.MapPost("/refresh", RefreshTokenAsync)
            .AllowAnonymous()
            .AddEndpointFilter(resultFilter)
            .WithName("RefreshToken");

        return group;
    }

    private static async Task<Result<AuthTokensResponse>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        ICommandHandler<RefreshTokenCommand, AuthTokensResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        return await handler.Handle(command, cancellationToken);
    }
}

internal sealed record RefreshTokenRequest(string RefreshToken);