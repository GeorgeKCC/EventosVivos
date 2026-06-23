using FluentAssertions;
using ModuloReserva.ImplementationUseCase;
using ModuloReservaContract.Dtos;
using Transversal.Database.Enums;

namespace ModuloReservaTest
{
    public class ValidationConfirmarReservaUseCaseTest
    {
        private readonly ValidationConfirmarReservaUseCase _validator = new();

        [Fact]
        public async Task Validate_RequestValido_DebeSerValido()
        {
            var request = new RequestEstadoReserva(EstadoReservaEnum.Confirmada, 1);
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_ReservaIdInvalido_DebeSerInvalido(int reservaId)
        {
            var request = new RequestEstadoReserva(EstadoReservaEnum.Confirmada, reservaId);
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ReservaId");
        }

        [Fact]
        public async Task Validate_EstadoEnumInvalido_DebeSerInvalido()
        {
            var request = new RequestEstadoReserva((EstadoReservaEnum)99, 1);
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "EstadoReservaEnum");
        }
    }
}
