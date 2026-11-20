using System.ComponentModel.DataAnnotations;

using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.Login;

internal static class LoginValidator
{
    public static Result Validate(LoginCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || !new EmailAddressAttribute().IsValid(command.Email))
        {
            return Result.Failure(DomainError.Validation(
                "Identity.InvalidEmail",
                "A valid email address is required."));           
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Result.Failure(DomainError.Validation(
                "Identity.PasswordRequired",
                "Password is required."));
        }

        return Result.Success();
    }
}
