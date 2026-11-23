using System.Windows.Input;

using SkillSync.Identity.Application.Common;
using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Identity.Application.Login;

internal sealed record LoginCommand(string Email, string Password) : ICommand<AuthTokensResponse>;
