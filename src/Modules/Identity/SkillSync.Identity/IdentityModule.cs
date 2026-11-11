using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SkillSync.Identity.Application.Abstractions;
using SkillSync.Identity.Application.Register;
using SkillSync.Identity.Endpoints;
using SkillSync.Identity.Infrastructure.Identity;
using SkillSync.Identity.Infrastructure.Persistence;
using SkillSync.Identity.Infrastructure.Tokens;
using SkillSync.SharedKernel.Messaging;


namespace SkillSync.Identity;

public static class IdentityModule
{
    internal const string RegistrationRateLimitPolicy = "registration";
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
        services.AddScoped<IIdentityUserService, IdentityUserService>();
        services.AddScoped<IdentityRoleSeeder>();
        services.AddScoped<
            ICommandHandler<RegisterCommand, RegisterResponse>, RegisterHandler>();

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(
                RegistrationRateLimitPolicy,
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));
        });

        return services;
    }

    public static async Task InitializeAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        await using var scope = serviceProvider.CreateAsyncScope();

        var roleSeeder = scope.ServiceProvider
            .GetRequiredService<IdentityRoleSeeder>();

        await roleSeeder.SeedAsync(cancellationToken);
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(
    this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return IdentityEndpoints.MapIdentityEndpoints(endpoints);
    }
}
