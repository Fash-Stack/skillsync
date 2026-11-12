using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SkillSync.Identity.Endpoints;

internal static class IdentityEndpoints
{
    internal static RouteGroupBuilder MapIdentityEndpointsInternal(
        this IEndpointRouteBuilder endpoints,
        IEndpointFilter resultFilter)
    {
        var authGroup = endpoints.MapGroup("/auth")
            .WithTags("Authentication");

        authGroup.MapRegisterEndpoint(resultFilter);

        return authGroup;
    }
}
