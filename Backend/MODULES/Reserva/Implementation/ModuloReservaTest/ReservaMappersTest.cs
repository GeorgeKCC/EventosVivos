using FluentAssertions;
using ModuloReserva.Mappers;
using ModuloReservaContract.Dtos;
using Transversal.Database.Enums;

namespace ModuloReservaTest
{
    public class ReservaMappersTest
    {
        [Fact]
        public void RequestCrearReservaToReserva_DebeMapearCorrectamente()
        {
            var request = new RequestCrearReserva(
                EventoId: 1,
                Cantidad: 5,
                NombreComprador: "Juan Pérez",
                EmailComprado: "juan@email.com"
            );

            var resultado = request.RequestCrearReservaToReserva();

            resultado.Cantidad.Should().Be(request.Cantidad);
            resultado.NombreComprador.Should().Be(request.NombreComprador);
            resultado.EmailComprador.Should().Be(request.EmailComprado);
            resultado.EventoId.Should().Be(request.EventoId);
            resultado.EstadoReservaId.Should().Be((int)EstadoReservaEnum.PendientePago);
        }

        [Fact]
        public void RequestCrearReservaToReserva_EstadoInicial_DebeSePendientePago()
        {
            var request = new RequestCrearReserva(1, 3, "Test", "test@mail.com");

            var resultado = request.RequestCrearReservaToReserva();

            resultado.EstadoReservaId.Should().Be((int)EstadoReservaEnum.PendientePago);
        }
    }
}
