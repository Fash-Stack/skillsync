using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Identity.Application.Register;

internal sealed record RegisterCommand(
    string Email,
    string Password) : ICommand<RegisterResponse>;
