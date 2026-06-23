using FluentAssertions;
using ModuloSecurity.ImplementationUseCase;
using ModuloSecurityContract.Dtos;

namespace ModuloSecurityTest
{
    public class ValidationLoginTest
    {
        private readonly ValidationLogin _validator = new();

        [Fact]
        public async Task Validate_RequestValido_DebeSerValido()
        {
            var request = new RequestLogin("admin", "admin");
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "admin")]
        [InlineData(null, "admin")]
        [InlineData("ab", "admin")]
        public async Task Validate_UsernameInvalido_DebeSerInvalido(string? username, string password)
        {
            var request = new RequestLogin(username!, password);
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Username");
        }

        [Theory]
        [InlineData("admin", "")]
        [InlineData("admin", null)]
        [InlineData("admin", "ab")]
        public async Task Validate_PasswordInvalido_DebeSerInvalido(string username, string? password)
        {
            var request = new RequestLogin(username, password!);
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }
    }
}
