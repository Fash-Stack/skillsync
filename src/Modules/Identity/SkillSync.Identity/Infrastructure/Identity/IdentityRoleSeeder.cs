using Microsoft.AspNetCore.Identity;

using SkillSync.SharedKernel.Security;

namespace SkillSync.Identity.Infrastructure.Identity;

internal sealed class IdentityRoleSeeder(
    RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        string[] roles =
        [
            Roles.Student,
            Roles.Admin,
            Roles.Instructor,
            Roles.Affiliate,
            Roles.Reviewer
        ];

        foreach (var role in roles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(role));

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to create the '{role}' role: {errors}");
            }
        }
    }
}
