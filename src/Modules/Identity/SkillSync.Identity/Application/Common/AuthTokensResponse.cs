namespace SkillSync.Identity.Application.Common;

internal sealed record AuthTokensResponse(string AccessToken, string RefreshToken);
