using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Domain;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Infrastructure.Identity;

internal sealed class IdentityUserService(
    UserManager<ApplicationUser> userManager,
    IdentityDbContext dbContext)
    : IIdentityUserService
{
    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);

            if (createResult.Errors.Any(error =>
            error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return Result.Failure<Guid>(IdentityErrors.DuplicateEmail);
            }

            return Result.Failure<Guid>(IdentityErrors.UserCreationFailed);
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<Guid>(IdentityErrors.RoleAssignmentFailed);
        }

        await transaction.CommitAsync(cancellationToken);

        return Result.Success(user.Id);
    }

    public async Task<Result<Guid>> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<Guid>(IdentityErrors.UserNotFound);

        return Result.Success(user.Id);
    }

    public async Task<Result<bool>> CheckPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Result.Failure<bool>(IdentityErrors.UserNotFound);

        if (await userManager.IsLockedOutAsync(user))
            return Result.Failure<bool>(IdentityErrors.AccountLocked);

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
                return Result.Failure<bool>(IdentityErrors.AccountLocked);

            return Result.Success(false);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return Result.Success(true);
    }

    public async Task<Result<IReadOnlyCollection<string>>> GetUserRoleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Result.Failure<IReadOnlyCollection<string>>(IdentityErrors.UserNotFound);

        var roles = await userManager.GetRolesAsync(user);

        return Result.Success<IReadOnlyCollection<string>>(roles.ToArray());
    }

    public async Task<Result> AddToRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Result.Failure(IdentityErrors.UserNotFound);

        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.RoleAssignmentFailed);

        return Result.Success();
    }
}
