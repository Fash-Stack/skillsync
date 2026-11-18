namespace SkillSync.Identity.Application.Abstractions;

internal interface ITokenService
{
    string CreateAccessToken(
        Guid userId,
        IEnumerable<string> roles);
}