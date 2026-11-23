using SkillSync.Identity.Infrastructure.Tokens;

namespace SkillSync.Identity.UnitTests.Infrastructure.Tokens;

public class TokenHasherTests
{
    [Fact]
    public void Hash_WithSameToken_ReturnsSameHash()
    {
        // Arrange
        var token = "sample-refresh-token";

        // Act
        var firstHash = TokenHasher.Hash(token);
        var secondHash = TokenHasher.Hash(token);

        // Assert
        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void Hash_WithValidToken_Returns64CharacterHexString()
    {
        // Arrange
        var token = "sample-refresh-token";

        // Act
        var hash = TokenHasher.Hash(token);

        // Assert
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9A-F]{64}$", hash);
    }

    [Fact]
    public void Verify_WithCorrectToken_ReturnsTrue()
    {
        // Arrange
        var token = "sample-refresh-token";
        var expectedHash = TokenHasher.Hash(token);

        // Act
        var result = TokenHasher.Verify(token, expectedHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_WithIncorrectToken_ReturnsFalse()
    {
        // Arrange
        var token = "sample-refresh-token";
        var incorrectToken = "another-refresh-token";
        var expectedHash = TokenHasher.Hash(token);

        // Act
        var result = TokenHasher.Verify(incorrectToken, expectedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithMalformedHash_ReturnsFalse()
    {
        // Arrange
        var token = "sample-refresh-token";
        var expectedHash = "NOT-A-VALID-HEX-HASH";

        // Act
        var result = TokenHasher.Verify(token, expectedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Hash_WithNullToken_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TokenHasher.Hash(null!));
    }

    [Fact]
    public void Verify_WithNullToken_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TokenHasher.Verify(null!, "ABC123"));
    }

    [Fact]
    public void Verify_WithNullExpectedHash_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TokenHasher.Verify("sample-refresh-token", null!));
    }

    [Fact]
    public void Hash_WithWhitespaceToken_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TokenHasher.Hash("   "));
    }

    [Fact]
    public void Verify_WithWhitespaceToken_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TokenHasher.Verify("   ", "ABC123"));
    }

    [Fact]
    public void Verify_WithWhitespaceExpectedHash_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TokenHasher.Verify("sample-refresh-token", "   "));
    }
}
