using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ModuloSecurity.ImplementationUseCase;
using ModuloSecurityContract;
using ModuloSecurityContract.Dtos;
using System.Text;

namespace ModuloSecurity
{
    public static class ModuloSecurityService
    {
        /// <summary>
        /// Registra los servicios del módulo de seguridad en el contenedor de dependencias.
        /// </summary>
        public static IServiceCollection ModuloSecurityRegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILoginUseCase, LoginUseCase>();
            services.AddScoped<ILogoutUseCase, LogoutUseCase>();
            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
            services.AddScoped<IValidator<RequestLogin>, ValidationLogin>();

            var jwtSettings = configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            services.AddAuthentication(options =>
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
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                        if (await blacklistService.IsTokenRevokedAsync(token))
                        {
                            context.Fail("Token revocado.");
                        }
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
