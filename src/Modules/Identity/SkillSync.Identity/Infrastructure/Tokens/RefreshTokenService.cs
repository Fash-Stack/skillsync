using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;

using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Application.Common;
using SkillSync.Identity.Domain;
using SkillSync.Identity.Infrastructure.Persistence;
using SkillSync.SharedKernel.Results;

namespace SkillSync.Identity.Infrastructure.Tokens;

internal sealed class RefreshTokenService(
    IdentityDbContext dbContext,
    JwtOptions jwtOptions
    ) : IRefreshTokenService
{
    public async Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rawToken = GenerateRawToken();

        var nowUtc = DateTimeOffset.UtcNow;

        var refreshToken = RefreshToken.Create(
            userId,
            Guid.NewGuid(),
            TokenHasher.Hash(rawToken),
            nowUtc,
            nowUtc.AddDays(jwtOptions.RefreshTokenLifeTimeInDays));

        dbContext.RefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    public async Task<Result<RotatedRefreshToken>> RotateAsync(string rawToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return Result.Failure<RotatedRefreshToken>(IdentityErrors.InvalidRefreshToken);
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var tokenHash = TokenHasher.Hash(rawToken);

        var existingToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken
            );

        if (existingToken is null)
        {
            return Result.Failure<RotatedRefreshToken>(IdentityErrors.InvalidRefreshToken);
        }

        var nowUtc = DateTimeOffset.UtcNow;

        if (existingToken.IsRevoked)
        {
            if (existingToken.ReplacedByTokenHash is not null)
            {
                var activeFamilyTokens = await dbContext.RefreshTokens
                    .Where(token =>
                        token.FamilyId == existingToken.FamilyId &&
                        token.RevokedAtUtc == null &&
                        token.ExpiresAtUtc > nowUtc)
                    .ToListAsync(cancellationToken);

                foreach (var familyToken in activeFamilyTokens)
                {
                    familyToken.Revoke(nowUtc);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            return Result.Failure<RotatedRefreshToken>(IdentityErrors.InvalidRefreshToken);
        }

        if (existingToken.IsExpired(nowUtc))
        {
            return Result.Failure<RotatedRefreshToken>(IdentityErrors.InvalidRefreshToken);
        }

        var replacementRawToken = GenerateRawToken();
        var replacementHash = TokenHasher.Hash(replacementRawToken);

        existingToken.Revoke(nowUtc, replacementHash);

        var replacementToken = RefreshToken.Create(
            existingToken.UserId,
            existingToken.FamilyId,
            replacementHash,
            nowUtc,
            nowUtc.AddDays(jwtOptions.RefreshTokenLifeTimeInDays));

        dbContext.RefreshTokens.Add(replacementToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<RotatedRefreshToken>(IdentityErrors.InvalidRefreshToken);
        }

        return Result.Success(
            new RotatedRefreshToken(
                existingToken.UserId,
                replacementRawToken
                ));
    }

    private static string GenerateRawToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
