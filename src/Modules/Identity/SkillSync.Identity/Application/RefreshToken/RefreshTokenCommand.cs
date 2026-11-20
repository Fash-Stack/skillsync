using SkillSync.Identity.Application.Common;
using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Identity.Application.RefreshToken;

internal sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokensResponse>;
