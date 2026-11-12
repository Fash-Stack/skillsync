using SkillSync.SharedKernel.Results;

namespace SkillSync.Api.Http;

internal sealed class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is Result applicationResult)
        {
            return applicationResult.ToHttpResult();
        }

        return result;
    }
}
