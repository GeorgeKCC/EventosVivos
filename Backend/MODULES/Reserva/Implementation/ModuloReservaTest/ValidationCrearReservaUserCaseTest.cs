using FluentAssertions;
using ModuloReserva.ImplementationUseCase;
using ModuloReservaContract.Dtos;

namespace ModuloReservaTest
{
    public class ValidationCrearReservaUserCaseTest
    {
        private readonly ValidationCrearReservaUserCase _validator = new();

        private static RequestCrearReserva CrearRequestValido() => new(
            EventoId: 1,
            Cantidad: 3,
            NombreComprador: "Juan Pérez",
            EmailComprado: "juan@email.com"
        );

        [Fact]
        public async Task Validate_RequestValido_DebeSerValido()
        {
            var request = CrearRequestValido();
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_EventoIdInvalido_DebeSerInvalido(int eventoId)
        {
            var request = CrearRequestValido() with { EventoId = eventoId };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "EventoId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_CantidadInvalida_DebeSerInvalido(int cantidad)
        {
            var request = CrearRequestValido() with { Cantidad = cantidad };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Cantidad");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ab")]
        public async Task Validate_NombreCompradorInvalido_DebeSerInvalido(string? nombre)
        {
            var request = CrearRequestValido() with { NombreComprador = nombre! };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "NombreComprador");
        }

        [Fact]
        public async Task Validate_NombreCompradorExcede100Caracteres_DebeSerInvalido()
        {
            var request = CrearRequestValido() with { NombreComprador = new string('A', 101) };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "NombreComprador");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("noesunmail")]
        public async Task Validate_EmailInvalido_DebeSerInvalido(string? email)
        {
            var request = CrearRequestValido() with { EmailComprado = email! };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "EmailComprado");
        }
    }
}
