using FluentAssertions;
using ModuloEvento.ImplementationUseCase;
using ModuloEventoContract.Dtos;

namespace ModuloEventoTest
{
    public class ValidationCrearEventoUseCaseTest
    {
        private readonly ValidationCrearEventoUseCase _validator = new();

        private static RequestCrearEvento CrearRequestValido() => new(
            Titulo: "Concierto de Rock",
            Descripcion: "Gran concierto de rock en vivo con bandas nacionales",
            CapacidadMaxima: 100,
            InicioEvento: new DateOnly(2025, 8, 15),
            IniciaHora: new TimeOnly(20, 0),
            FinEvento: new DateOnly(2025, 8, 15),
            FinHora: new TimeOnly(23, 0),
            Precio: 150m,
            VenueId: 1,
            TipoEventoId: 1
        );

        [Fact]
        public async Task Validate_RequestValido_DebeSerValido()
        {
            var request = CrearRequestValido();
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        public async Task Validate_TituloInvalido_DebeSerInvalido(string? titulo)
        {
            var request = CrearRequestValido() with { Titulo = titulo! };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
        }

        [Fact]
        public async Task Validate_TituloExcede100Caracteres_DebeSerInvalido()
        {
            var request = CrearRequestValido() with { Titulo = new string('A', 101) };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("corta")]
        public async Task Validate_DescripcionInvalida_DebeSerInvalido(string? descripcion)
        {
            var request = CrearRequestValido() with { Descripcion = descripcion! };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
        }

        [Fact]
        public async Task Validate_DescripcionExcede500Caracteres_DebeSerInvalido()
        {
            var request = CrearRequestValido() with { Descripcion = new string('A', 501) };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_CapacidadMaximaInvalida_DebeSerInvalido(int capacidad)
        {
            var request = CrearRequestValido() with { CapacidadMaxima = capacidad };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CapacidadMaxima");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_VenueIdInvalido_DebeSerInvalido(int venueId)
        {
            var request = CrearRequestValido() with { VenueId = venueId };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "VenueId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_PrecioInvalido_DebeSerInvalido(decimal precio)
        {
            var request = CrearRequestValido() with { Precio = precio };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Precio");
        }
    }
}
