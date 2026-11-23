using System.Security.Cryptography;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using SkillSync.Identity.Infrastructure.Tokens;

namespace SkillSync.Identity.UnitTests.Infrastructure.Tokens;

public class JwtTokenServiceTests
{
    private static JwtOptions CreateValidOptions()
    {
        return new JwtOptions
        {
            Issuer = "SkillSync",
            Audience = "SkillSync.Api",
            SecretKey = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32)),
            AccessTokenLifetimeMinutes = 15
        };
    }

    [Fact]
    public void CreateAccessToken_WithValidValues_ReturnsToken()
    {
        // Arrange
        var service = new JwtTokenService(CreateValidOptions());
        var userId = Guid.NewGuid();
        var roles = new[] { "Student" };

        // Act
        var token = service.CreateAccessToken(userId, roles);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void CreateAccessToken_WithValidValues_ContainsUserIdClaim()
    {
        // Arrange
        var service = new JwtTokenService(CreateValidOptions());
        var userId = Guid.NewGuid();

        // Act
        var token = service.CreateAccessToken(userId, ["Student"]);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        // Assert
        Assert.Equal(userId.ToString(), jwt.Subject);
    }

    [Fact]
    public void CreateAccessToken_WithValidValues_ContainsIssuerAndAudience()
    {
        // Arrange
        var options = CreateValidOptions();
        var service = new JwtTokenService(options);

        // Act
        var token = service.CreateAccessToken(Guid.NewGuid(), ["Student"]);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        // Assert
        Assert.Equal(options.Issuer, jwt.Issuer);
        Assert.Contains(options.Audience, jwt.Audiences);
    }

    [Fact]
    public void CreateAccessToken_WithRoles_ContainsRoleClaims()
    {
        // Arrange
        var service = new JwtTokenService(CreateValidOptions());
        var roles = new[] { "Student", "Instructor" };

        // Act
        var token = service.CreateAccessToken(Guid.NewGuid(), roles);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        // Assert
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == "role" && claim.Value == "Student");

        Assert.Contains(jwt.Claims, claim =>
            claim.Type == "role" && claim.Value == "Instructor");
    }

    [Fact]
    public void CreateAccessToken_GeneratesUniqueTokenIds()
    {
        // Arrange
        var service = new JwtTokenService(CreateValidOptions());
        var userId = Guid.NewGuid();

        // Act
        var firstToken = new JsonWebTokenHandler()
            .ReadJsonWebToken(service.CreateAccessToken(userId, []));

        var secondToken = new JsonWebTokenHandler()
            .ReadJsonWebToken(service.CreateAccessToken(userId, []));

        // Assert
        Assert.NotEqual(
            firstToken.Id,
            secondToken.Id);
    }

    [Fact]
    public void CreateAccessToken_UsesConfiguredExpiry()
    {
        // Arrange
        var options = CreateValidOptions();
        var service = new JwtTokenService(options);
        var beforeCreation = DateTime.UtcNow;

        // Act
        var token = service.CreateAccessToken(Guid.NewGuid(), []);
        var afterCreation = DateTime.UtcNow;
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        // Assert
        Assert.InRange(
             jwt.ValidTo,
             beforeCreation.AddMinutes(options.AccessTokenLifetimeMinutes)
                 .AddSeconds(-1),
             afterCreation.AddMinutes(options.AccessTokenLifetimeMinutes));
    }

    [Fact]
    public void CreateAccessToken_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new JwtTokenService(CreateValidOptions());

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            service.CreateAccessToken(Guid.Empty, ["Student"]));
    }

    [Fact]
    public void CreateAccessToken_WithNullRoles_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new JwtTokenService(CreateValidOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            service.CreateAccessToken(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task CreateAccessToken_WithValidConfiguration_ProducesValidSignature()
    {
        // Arrange
        var options = CreateValidOptions();
        var service = new JwtTokenService(options);

        // Act
        var token = service.CreateAccessToken(
            Guid.NewGuid(),
            ["Student"]);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                options.GetSigningKey()),

            ValidateIssuer = true,
            ValidIssuer = options.Issuer,

            ValidateAudience = true,
            ValidAudience = options.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Assert
        var result = await new JsonWebTokenHandler()
            .ValidateTokenAsync(token, validationParameters);

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task CreateAccessToken_WithWrongSigningKey_FailsValidation()
    {
        // Arrange
        var options = CreateValidOptions();
        var service = new JwtTokenService(options);

        var token = service.CreateAccessToken(
            Guid.NewGuid(),
            ["Student"]);

        var wrongKey = RandomNumberGenerator.GetBytes(32);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(wrongKey),
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var result = await new JsonWebTokenHandler()
            .ValidateTokenAsync(token, validationParameters);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task CreateAccessToken_WithWrongAudience_FailsValidation()
    {
        // Arrange
        var options = CreateValidOptions();
        var service = new JwtTokenService(options);

        var token = service.CreateAccessToken(
            Guid.NewGuid(),
            ["Student"]);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                options.GetSigningKey()),
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = "Another.Api",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var result = await new JsonWebTokenHandler()
            .ValidateTokenAsync(token, validationParameters);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task CreateAccessToken_WithWrongIssuer_FailsValidation()
    {
        // Arrange
        var options = CreateValidOptions();
        var service = new JwtTokenService(options);

        var token = service.CreateAccessToken(
            Guid.NewGuid(),
            ["Student"]);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                options.GetSigningKey()),
            ValidateIssuer = true,
            ValidIssuer = "Another.Issuer",
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var result = await new JsonWebTokenHandler()
            .ValidateTokenAsync(token, validationParameters);

        // Assert
        Assert.False(result.IsValid);
    }
}