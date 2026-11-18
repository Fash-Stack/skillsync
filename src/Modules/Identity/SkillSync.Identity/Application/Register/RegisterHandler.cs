using SkillSync.Identity.Application.Abstractions;
using SkillSync.SharedKernel.Messaging;
using SkillSync.SharedKernel.Results;
using SkillSync.SharedKernel.Security;

namespace SkillSync.Identity.Application.Register;

internal sealed class RegisterHandler(
    IIdentityUserService identityUserService) 
    : ICommandHandler<RegisterCommand, RegisterResponse>
{
    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = RegisterValidator.Validate(command);

        if (validationResult.IsFailure)
        {
            return Result.Failure<RegisterResponse>(validationResult.Error);
        }

        var createUserResult = await identityUserService.CreateUserAsync(
            command.Email,
            command.Password,
            Roles.Student,
            cancellationToken);

        if (createUserResult.IsFailure)
            return Result.Failure<RegisterResponse>(createUserResult.Error);

        return Result.Success(
            new RegisterResponse(createUserResult.Value));
    }
}
