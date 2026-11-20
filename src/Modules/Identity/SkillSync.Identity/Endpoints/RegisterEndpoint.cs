using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SkillSync.Identity.Application.Register;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Endpoints;

internal static class RegisterEndpoint
{
    internal static RouteGroupBuilder MapRegisterEndpoint(
        this RouteGroupBuilder group,
        IEndpointFilter resultFilter)
    {
        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .RequireRateLimiting("registration")
            .AddEndpointFilter(resultFilter)
            .WithName("Register");

        return group;
    }
    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        ICommandHandler<RegisterCommand, RegisterResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password);

        return (IResult)await handler.Handle(command, cancellationToken);
    }
}

internal sealed record RegisterRequest(
    string Email,
    string Password);
