using System.Text;
using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using AtermisShop.Infrastructure.Auth;
using AtermisShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace AtermisShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null));
            // Don't validate connection on startup to allow Swagger to work even if DB is down
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

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
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = key
                };
            });

        services.AddScoped<IJwtTokenService>(sp => new Auth.JwtTokenService(configuration));

        // Register payment providers
        services.AddHttpClient<Payments.MomoPaymentProvider>();
        services.AddScoped<Application.Payments.Common.IPaymentProvider, Payments.MomoPaymentProvider>();
        services.AddScoped<Application.Payments.Common.IPaymentProvider, Payments.VnPayPaymentProvider>();
        services.AddHttpClient<Payments.PayOsPaymentProvider>();
        services.AddScoped<Application.Payments.Common.IPaymentProvider, Payments.PayOsPaymentProvider>();

        return services;
    }
}

