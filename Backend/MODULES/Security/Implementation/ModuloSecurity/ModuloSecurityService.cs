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
            });

            services.AddAuthorization();

            return services;
        }
    }
}
