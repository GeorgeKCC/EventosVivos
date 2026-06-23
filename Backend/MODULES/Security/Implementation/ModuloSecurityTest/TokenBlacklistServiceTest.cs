using FluentAssertions;
using ModuloSecurity.ImplementationUseCase;

namespace ModuloSecurityTest
{
    public class TokenBlacklistServiceTest
    {
        private readonly TokenBlacklistService _service = new();

        [Fact]
        public async Task RevokeTokenAsync_TokenValido_DebeAgregarAListaNegra()
        {
            var token = "test-token-123";

            await _service.RevokeTokenAsync(token);

            var isRevoked = await _service.IsTokenRevokedAsync(token);
            isRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task IsTokenRevokedAsync_TokenNoRevocado_DebeRetornarFalse()
        {
            var token = "token-no-revocado";

            var isRevoked = await _service.IsTokenRevokedAsync(token);

            isRevoked.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeTokenAsync_MismosTokenDosVeces_NoDebeLanzarExcepcion()
        {
            var token = "token-duplicado";

            await _service.RevokeTokenAsync(token);
            var act = () => _service.RevokeTokenAsync(token);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task IsTokenRevokedAsync_MultiplesTokensRevocados_DebeVerificarCorrectamente()
        {
            var token1 = "token-1";
            var token2 = "token-2";
            var token3 = "token-3";

            await _service.RevokeTokenAsync(token1);
            await _service.RevokeTokenAsync(token2);

            (await _service.IsTokenRevokedAsync(token1)).Should().BeTrue();
            (await _service.IsTokenRevokedAsync(token2)).Should().BeTrue();
            (await _service.IsTokenRevokedAsync(token3)).Should().BeFalse();
        }
    }
}
