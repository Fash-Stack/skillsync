using SkillSync.SharedKernel.Results;

namespace SkillSync.Api.Http;

internal static class ResultExtensions
{
    internal static IResult ToHttpResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

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

        if (result is IValueResult valueResult)
        {
            return Results.Ok(valueResult.Value);
        }
        
        return Results.NoContent();
    }
}
