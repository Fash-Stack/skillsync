using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SkillSync.Identity.Application.Common;
using SkillSync.Identity.Application.Login;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Endpoints;

internal static class LoginEndpoints
{
    internal static RouteGroupBuilder MapLoginEndpoint(
        this RouteGroupBuilder group,
        IEndpointFilter resultFilter)
    {
        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .AddEndpointFilter(resultFilter)
            .WithName("Login");

        return group;
    }

    private static async Task<Result<AuthTokensResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        ICommandHandler<LoginCommand, AuthTokensResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        return await handler.Handle(command, cancellationToken);
    }
}

internal sealed record LoginRequest(string Email, string Password);
