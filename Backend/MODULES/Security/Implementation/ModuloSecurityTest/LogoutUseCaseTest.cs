using FluentAssertions;
using ModuloSecurity.ImplementationUseCase;
using ModuloSecurityContract;
using Moq;

namespace ModuloSecurityTest
{
    public class LogoutUseCaseTest
    {
        private readonly Mock<ITokenBlacklistService> _blacklistServiceMock = new();
        private readonly LogoutUseCase _logoutUseCase;

        public LogoutUseCaseTest()
        {
            _logoutUseCase = new LogoutUseCase(_blacklistServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_TokenValido_DebeRevocarToken()
        {
            var token = "valid-jwt-token";
            _blacklistServiceMock.Setup(x => x.RevokeTokenAsync(token)).Returns(Task.CompletedTask);

            await _logoutUseCase.ExecuteAsync(token);

            _blacklistServiceMock.Verify(x => x.RevokeTokenAsync(token), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExecuteAsync_TokenNuloOVacio_DebeLanzarUnauthorizedAccessException(string? token)
        {
            var act = () => _logoutUseCase.ExecuteAsync(token!);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Token no proporcionado.");
        }
    }
}
