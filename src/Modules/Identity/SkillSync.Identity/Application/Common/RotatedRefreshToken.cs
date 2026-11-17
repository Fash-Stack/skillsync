namespace SkillSync.Identity.Application.Common;

internal sealed record RotatedRefreshToken(Guid UserId, string RawToken);
