using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModuloSecurityContract;
using ModuloSecurityContract.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Transversal.Database;

namespace ModuloSecurity.ImplementationUseCase
{
    internal class LoginUseCase(EventosVivosDbContext dbContext,
                                IConfiguration configuration,
                                IValidator<RequestLogin> validator) : ILoginUseCase
    {
        public async Task<ResponseLogin> ExecuteAsync(RequestLogin request)
        {
            var validate = await validator.ValidateAsync(request);
            if (validate.IsValid is false)
            {
                throw new ValidationException(validate.Errors);
            }

            var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

            var usuario = await dbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == passwordHash)
                ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

            var token = GenerateToken(usuario.Username, usuario.Rol.Nombre);

            return new ResponseLogin(token, usuario.Username, usuario.Rol.Nombre);
        }

        private string GenerateToken(string username, string rol)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, rol)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    internal class ValidationLogin : AbstractValidator<RequestLogin>
    {
        public ValidationLogin()
        {
            RuleFor(x => x.Username).NotNull().NotEmpty().MinimumLength(3);
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(3);
        }
    }
}
