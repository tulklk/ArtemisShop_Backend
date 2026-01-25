using System.Security.Claims;
using System.Text;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using AtermisShop.Infrastructure.Auth;
using AtermisShop.Infrastructure.Persistence;
using AtermisShop.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace AtermisShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Log the connection attempt (masking password)
        if (!string.IsNullOrEmpty(connectionString))
        {
            var maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=[^;]+", "Password=******");
            Console.WriteLine($"[DB Setup] Attempting to connect with: {maskedConnectionString}");
        }
        else
        {
            Console.WriteLine("[DB Setup] No connection string found in configuration!");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });
            // Don't validate connection on startup to allow Swagger to work even if DB is down
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register custom UserService to replace Identity
        services.AddScoped<IUserService, UserService>();

        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = key,
                    // Map claim types correctly so [Authorize(Roles = "Admin")] works
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                    // Allow small clock skew to handle minor time differences
                    ClockSkew = TimeSpan.FromMinutes(5),
                    // Map role from integer to string for [Authorize(Roles = "Admin")]
                    // Role will be added as "Admin" claim in JWT token based on user.Role == 1
                    SaveSigninToken = false
                };
                
                // Ensure role claims are properly mapped from JWT
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        var tokenPreview = authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader;
                        
                        logger?.LogError(context.Exception, 
                            "JWT Authentication failed. Error: {Error}, Token preview: {TokenPreview}", 
                            context.Exception.Message, tokenPreview);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        var tokenPreview = authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader;
                        
                        logger?.LogWarning("JWT Challenge: {Error}, {ErrorDescription}, Token preview: {TokenPreview}", 
                            context.Error, context.ErrorDescription, tokenPreview);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // This ensures role claims are available for authorization
                        if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                        {
                            // Role claims should already be mapped by RoleClaimType setting above
                            // But we can verify they exist for debugging
                            var roles = claimsIdentity.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                            logger?.LogInformation("Token validated. User: {UserId}, Roles: {Roles}", 
                                claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value, 
                                string.Join(", ", roles));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddScoped<IJwtTokenService, Auth.JwtTokenService>();

        // Register email service
        services.AddScoped<Application.Common.Interfaces.IEmailService, EmailService>();

        // Register email verification token service
        services.AddScoped<Application.Common.Interfaces.IEmailVerificationTokenService, EmailVerificationTokenService>();

        // Register password reset token service
        services.AddScoped<Application.Common.Interfaces.IPasswordResetTokenService, PasswordResetTokenService>();

        // Register payment providers
        services.AddScoped<Application.Payments.Common.IPaymentProvider, Payments.PayOsPaymentProvider>();

        // Register Gemini AI service
        services.AddHttpClient<Application.Common.Interfaces.IGeminiService, Services.GeminiService>();

        // Register HttpClientFactory for Facebook OAuth
        services.AddHttpClient();

        // Register RSS Feed Service
        services.AddHttpClient<IRssFeedService, RssFeedService>();

        return services;
    }
}

