using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.Abstractions;

internal interface IIdentityUserService
{
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string role,
        CancellationToken cancellationToken);

    Task<Result<Guid>> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result<bool>> CheckPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<string>>> GetUserRoleAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result> AddToRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken);
}
