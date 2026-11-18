using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SkillSync.Identity.Application.Logout;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Endpoints;

internal static class LogoutEndpoint
{
    internal static RouteGroupBuilder MapLogoutEndpoint(
        this RouteGroupBuilder group,
        IEndpointFilter filter)
    {
        group.MapPost("/logout", LogoutAsync)
            .AllowAnonymous()
            .AddEndpointFilter(filter)
            .WithName("Logout");

        return group;
    }

    private static async Task<Result> LogoutAsync(
        [FromBody] LogoutRequest request,
        ICommandHandler<LogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken);

        return await handler.Handle(command, cancellationToken);
    }
}

internal sealed record LogoutRequest(string RefreshToken);