using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Identity.Application.Logout;

internal sealed record LogoutCommand(string RefreshToken) : ICommand;