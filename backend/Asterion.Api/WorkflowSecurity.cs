using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Asterion.Api;

public static class WorkflowSecurity
{
    public static IServiceCollection AddWorkflowSecurity(
        this IServiceCollection services, IConfiguration config)
    {
        var key = config["Auth:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Set Auth:SigningKey to at least 32 random bytes");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = config["Auth:Issuer"] ?? "asterion-local",
                ValidateAudience = true,
                ValidAudience = config["Auth:Audience"] ?? "asterion-api",
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromSeconds(10)
            };
        });
        services.AddAuthorizationBuilder()
            .AddPolicy("tickets.read", policy => policy.RequireAuthenticatedUser()
                .RequireClaim("scope", "tickets.read", "tickets.write"))
            .AddPolicy("tickets.write", policy => policy.RequireAuthenticatedUser()
                .RequireClaim("scope", "tickets.write"));
        return services;
    }
}
