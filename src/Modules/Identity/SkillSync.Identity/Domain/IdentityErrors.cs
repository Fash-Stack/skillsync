using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Domain;

internal static class IdentityErrors
{
    public static readonly DomainError DuplicateEmail = DomainError.Conflict(
        "Identity.DuplicateEmail",
        "An account with this email already exists.");

    public static readonly DomainError UserNotFound = DomainError.NotFound(
        "Identity.UserNotFound",
        "The requested user could not be found.");

    public static readonly DomainError InvalidCredentials = DomainError.Unauthorized(
        "Identity.InvalidCredentials",
        "The supplied credentials are invalid");

    public static readonly DomainError AccountLocked = DomainError.Unauthorized(
        "Identity.AccountLocked",
        "This account is temporarily locked.");

    public static readonly DomainError UserCreationFailed = DomainError.Failure(
        "Identity.UserCreationFailed",
        "The account creation failed.");

    public static readonly DomainError RoleAssignmentFailed = DomainError.Failure(
        "Identity.RoleAssignmentFailed",
        "The requested role could not be assigned.");

    public static readonly DomainError RoleRetrievalFailed = DomainError.Failure(
        "Identity.RoleRetrievalFailed",
        "The user's roles could not be retrieved.");
}
