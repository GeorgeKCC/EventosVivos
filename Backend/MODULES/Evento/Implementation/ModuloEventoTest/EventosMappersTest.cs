using FluentAssertions;
using ModuloEvento.Mappers;
using ModuloEventoContract.Dtos;
using Transversal.Database.Entities;
using Transversal.Database.Enums;

namespace ModuloEventoTest
{
    public class EventosMappersTest
    {
        [Fact]
        public void RequestCrearEventoToEvento_DebeMapearCorrectamente()
        {
            var request = new RequestCrearEvento(
                Titulo: "Concierto de Rock",
                Descripcion: "Gran concierto de rock en vivo",
                CapacidadMaxima: 100,
                InicioEvento: new DateOnly(2025, 8, 15),
                IniciaHora: new TimeOnly(20, 0),
                FinEvento: new DateOnly(2025, 8, 15),
                FinHora: new TimeOnly(23, 0),
                Precio: 150m,
                VenueId: 1,
                TipoEventoId: 2
            );

            var resultado = request.RequestCrearEventoToEvento();

            resultado.Titulo.Should().Be(request.Titulo);
            resultado.Descripcion.Should().Be(request.Descripcion);
            resultado.CapacidadMaxima.Should().Be(request.CapacidadMaxima);
            resultado.InicioEvento.Should().Be(request.InicioEvento);
            resultado.IniciaHora.Should().Be(request.IniciaHora);
            resultado.FinEvento.Should().Be(request.FinEvento);
            resultado.FinHora.Should().Be(request.FinHora);
            resultado.Precio.Should().Be(request.Precio);
            resultado.VenueId.Should().Be(request.VenueId);
            resultado.TipoEventoId.Should().Be(request.TipoEventoId);
            resultado.EstadoId.Should().Be((int)EstadoEnum.Activo);
        }

        [Fact]
        public void EventoToResponseBuscarEvento_DebeMapearCorrectamente()
        {
            var evento = new Evento
            {
                Id = 1,
                Titulo = "Concierto de Rock",
                Descripcion = "Gran concierto",
                CapacidadMaxima = 100,
                InicioEvento = new DateOnly(2025, 8, 15),
                IniciaHora = new TimeOnly(20, 0),
                FinEvento = new DateOnly(2025, 8, 15),
                FinHora = new TimeOnly(23, 0),
                Precio = 150m,
                VenueId = 1,
                Venue = new Venue { Id = 1, Nombre = "Arena CDMX", Capacidad = 200, Ciudad = "CDMX" },
                TipoEventoId = 2,
                TipoEvento = new TipoEvento { Id = 2, Nombre = "Concierto" },
                EstadoId = 1,
                EstadoEvento = new EstadoEvento { Id = 1, Nombre = "Activo" }
            };

            var resultado = evento.EventoToResponseBuscarEvento();

            resultado.Titulo.Should().Be(evento.Titulo);
            resultado.Descripción.Should().Be(evento.Descripcion);
            resultado.VenuedId.Should().Be(evento.VenueId);
            resultado.VenueName.Should().Be("Arena CDMX");
            resultado.CapacidadMaxima.Should().Be(evento.CapacidadMaxima);
            resultado.FechaInicioEvento.Should().Be(evento.InicioEvento);
            resultado.HoraInicioEvento.Should().Be(evento.IniciaHora);
            resultado.FechaFinEvento.Should().Be(evento.FinEvento);
            resultado.HoraFinEvento.Should().Be(evento.FinHora);
            resultado.TipoEventoId.Should().Be(evento.TipoEventoId);
            resultado.TipoEventoNombre.Should().Be("Concierto");
            resultado.EstadoId.Should().Be(evento.EstadoId);
            resultado.EstadoNombre.Should().Be("Activo");
        }
    }
}
