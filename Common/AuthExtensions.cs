using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public static class AuthExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication()
            .AddKeycloakJwtBearer(serviceName: "keycloak-svc", realm: "overflow", options =>
            {
                options.RequireHttpsMetadata = false;
                options.Audience = "overflow";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers =
                    [
                        "http://id.overflow.local/realms/overflow",
                        "http://localhost:6001/realms/overflow",
                        "http://keycloak/realms/overflow"
                    ],
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return services;
    }
}