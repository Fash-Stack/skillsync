using System.ComponentModel.DataAnnotations;

using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Application.Register;

internal static class RegisterValidator
{
    public static Result Validate(RegisterCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email) ||
             !new EmailAddressAttribute().IsValid(command.Email))
        {
            return Result.Failure(DomainError.Validation(
                "Identity.InvalidEmail",
                "A valid email address is required."));
        }

        var password = command.Password;

        var isPasswordValid = !string.IsNullOrWhiteSpace(password) &&
            password.Length >= 8 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit) &&
            password.Any(character => !char.IsLetterOrDigit(character));

        if (!isPasswordValid)
        {
            return Result.Failure(DomainError.Validation(
                "Identity.WeakPassword",
                "Password must contain at least 8 characters, including uppercase and lowercase letters, a digit, and a special character."));
        }

        return Result.Success();
    }
}
