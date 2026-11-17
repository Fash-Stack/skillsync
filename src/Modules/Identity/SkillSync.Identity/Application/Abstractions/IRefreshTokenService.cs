using SkillSync.Identity.Application.Common;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.Abstractions;

internal interface IRefreshTokenService
{
    Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<RotatedRefreshToken>> RotateAsync(string rawToken, CancellationToken cancellationToken);
}
