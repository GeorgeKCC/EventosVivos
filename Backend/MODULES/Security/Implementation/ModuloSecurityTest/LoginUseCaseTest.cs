using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModuloSecurity.ImplementationUseCase;
using ModuloSecurityContract.Dtos;
using System.Security.Cryptography;
using System.Text;
using Transversal.Database;
using Transversal.Database.Entities;

namespace ModuloSecurityTest
{
    public class LoginUseCaseTest
    {
        private readonly EventosVivosDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ValidationLogin _validator = new();
        private readonly LoginUseCase _loginUseCase;

        public LoginUseCaseTest()
        {
            var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new EventosVivosDbContext(options);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "EventosVivos_SuperSecretKey_2024_MinLength32Chars!" },
                { "Jwt:Issuer", "EventosVivos.API" },
                { "Jwt:Audience", "EventosVivos.Client" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            SeedData();
            _loginUseCase = new LoginUseCase(_dbContext, _configuration, _validator);
        }

        private void SeedData()
        {
            var rol = new Rol { Id = 1, Nombre = "Admin" };
            _dbContext.Roles.Add(rol);

            var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("admin")));
            _dbContext.Usuarios.Add(new Usuario
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash,
                RolId = 1
            });

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task ExecuteAsync_CredencialesValidas_DebeRetornarToken()
        {
            var request = new RequestLogin("admin", "admin");

            var result = await _loginUseCase.ExecuteAsync(request);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Username.Should().Be("admin");
            result.Rol.Should().Be("Admin");
        }

        [Fact]
        public async Task ExecuteAsync_PasswordIncorrecto_DebeLanzarUnauthorizedAccessException()
        {
            var request = new RequestLogin("admin", "wrongpassword");

            var act = () => _loginUseCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Credenciales inválidas.");
        }

        [Fact]
        public async Task ExecuteAsync_UsuarioNoExiste_DebeLanzarUnauthorizedAccessException()
        {
            var request = new RequestLogin("noexiste", "admin");

            var act = () => _loginUseCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Credenciales inválidas.");
        }

        [Fact]
        public async Task ExecuteAsync_RequestInvalido_DebeLanzarValidationException()
        {
            var request = new RequestLogin("", "");

            var act = () => _loginUseCase.ExecuteAsync(request);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
