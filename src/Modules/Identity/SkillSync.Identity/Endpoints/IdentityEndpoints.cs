using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SkillSync.Identity.Application.Register;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Endpoints;

internal static class IdentityEndpoints
{
    internal static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity")
            .WithTags("Identity");

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .RequireRateLimiting(IdentityModule.RegistrationRateLimitPolicy)
            .WithName("Register");

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        ICommandHandler<RegisterCommand, RegisterResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            return Results.Problem(
                statusCode: statusCode,
                title: result.Error.Code,
                detail: result.Error.Description);
        }

        return Results.Ok(new { result.Value.UserId });
    }
}

internal sealed record RegisterRequest(
    string Email,
    string Password);
