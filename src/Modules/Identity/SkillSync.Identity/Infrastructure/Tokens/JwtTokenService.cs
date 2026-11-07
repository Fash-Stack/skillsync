using System.Security.Claims;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using SkillSync.Identity.Application.Abstractions;

namespace SkillSync.Identity.Infrastructure.Tokens;

internal sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(JwtOptions options)
    {
        options.Validate();

        _options = options;

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(options.GetSigningKey()),
            SecurityAlgorithms.HmacSha256);
    }

    public string CreateAccessToken(
        Guid userId,
        IEnumerable<string> roles)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ArgumentNullException.ThrowIfNull(roles);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(
            roles.Select(role => new Claim("role", role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                _options.AccessTokenLifetimeMinutes),
            SigningCredentials = _signingCredentials
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}