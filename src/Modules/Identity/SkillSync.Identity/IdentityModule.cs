using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Infrastructure;
using SkillSync.Identity.Infrastructure.Tokens;
using Microsoft.AspNetCore.Identity;
using SkillSync.Identity.Infrastructure.Identity;


namespace SkillSync.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var jwtOptions = new JwtOptions 
        { 
            Issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is missing."),
            Audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT audience is missing."),
            SecretKey = configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT secret key is missing."),
        };

        jwtOptions.Validate();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' is missing.");

        services.AddSingleton(jwtOptions);
        services.AddSingleton<ITokenService, JwtTokenService>();
        
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>();

        return services;
    }
}
