using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModuloEventoContract;
using ModuloEventoContract.Dtos;
using ModuloReservaContract;
using ModuloReservaContract.Dtos;
using Transversal.Database;
using Transversal.Database.Enums;

namespace FlowTests
{
    /// <summary>
    /// Tests de flujo completo del sistema:
    /// RF-01: Crear Evento → RF-02: Reservar Entrada → RF-04: Confirmar Pago de Reserva
    /// </summary>
    public class FlujoCompletoTest : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public FlujoCompletoTest()
        {
            _serviceProvider = FlowTestServiceProvider.Create(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// RF-01 → RF-02 → RF-04: Flujo completo exitoso.
        /// Crea un evento, reserva entradas y confirma el pago.
        /// </summary>
        [Fact]
        public async Task FlujoCompleto_CrearEvento_ReservarEntrada_ConfirmarPago_DebeSerExitoso()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();
            var crearReserva = scope.ServiceProvider.GetRequiredService<ICrearReservaUserCase>();
            var confirmarReserva = scope.ServiceProvider.GetRequiredService<IConfirmarReservaUseCase>();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            // RF-01: Crear Evento
            var requestEvento = new RequestCrearEvento(
                Titulo: "Concierto de Rock Internacional",
                Descripcion: "Gran concierto de rock con bandas internacionales en vivo",
                CapacidadMaxima: 200,
                InicioEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                IniciaHora: new TimeOnly(19, 0),
                FinEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                FinHora: new TimeOnly(23, 0),
                Precio: 80m,
                VenueId: 1,
                TipoEventoId: 1
            );

            await crearEvento.ExecuteAsync(requestEvento);

            var eventoCreado = await context.Eventos.FirstOrDefaultAsync(e => e.Titulo == "Concierto de Rock Internacional");
            eventoCreado.Should().NotBeNull();
            eventoCreado!.EstadoId.Should().Be((int)EstadoEnum.Activo);
            eventoCreado.CapacidadMaxima.Should().Be(200);

            // RF-02: Reservar Entrada
            var requestReserva = new RequestCrearReserva(
                EventoId: eventoCreado.Id,
                Cantidad: 3,
                NombreComprador: "Juan Pérez",
                EmailComprado: "juan.perez@email.com"
            );

            await crearReserva.ExecuteAsync(requestReserva);

            var reservaCreada = await context.Reservas.FirstOrDefaultAsync(r => r.EventoId == eventoCreado.Id);
            reservaCreada.Should().NotBeNull();
            reservaCreada!.Cantidad.Should().Be(3);
            reservaCreada.EstadoReservaId.Should().Be((int)EstadoReservaEnum.PendientePago);

            // RF-04: Confirmar Pago de Reserva
            var requestConfirmar = new RequestEstadoReserva(
                EstadoReservaEnum: EstadoReservaEnum.Confirmada,
                ReservaId: reservaCreada.Id
            );

            var codigoReserva = await confirmarReserva.ExecuteAsync(requestConfirmar);

            codigoReserva.Should().NotBeNullOrWhiteSpace();
            codigoReserva.Should().StartWith("EV-");

            var reservaConfirmada = await context.Reservas.FirstAsync(r => r.Id == reservaCreada.Id);
            reservaConfirmada.EstadoReservaId.Should().Be((int)EstadoReservaEnum.Confirmada);
        }

        /// <summary>
        /// RF-01 → RF-02 (múltiples): Varias reservas para un mismo evento
        /// hasta agotar capacidad.
        /// </summary>
        [Fact]
        public async Task FlujoCompleto_MultiplesReservas_DebeRespetarCapacidad()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();
            var crearReserva = scope.ServiceProvider.GetRequiredService<ICrearReservaUserCase>();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            // RF-01: Crear Evento con capacidad limitada
            var requestEvento = new RequestCrearEvento(
                Titulo: "Taller Exclusivo de Programación",
                Descripcion: "Taller intensivo de programación avanzada en .NET",
                CapacidadMaxima: 10,
                InicioEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(15)),
                IniciaHora: new TimeOnly(9, 0),
                FinEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(15)),
                FinHora: new TimeOnly(17, 0),
                Precio: 50m,
                VenueId: 1,
                TipoEventoId: 1
            );

            await crearEvento.ExecuteAsync(requestEvento);
            var evento = await context.Eventos.FirstAsync(e => e.Titulo == "Taller Exclusivo de Programación");

            // RF-02: Primera reserva (5 entradas)
            await crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 5,
                NombreComprador: "María García",
                EmailComprado: "maria.garcia@email.com"
            ));

            // RF-02: Segunda reserva (4 entradas)
            await crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 4,
                NombreComprador: "Carlos López",
                EmailComprado: "carlos.lopez@email.com"
            ));

            var totalReservadas = await context.Reservas
                .Where(r => r.EventoId == evento.Id)
                .SumAsync(r => r.Cantidad);
            totalReservadas.Should().Be(9);

            // RF-02: Tercera reserva que excede capacidad (debe fallar)
            var act = () => crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 3,
                NombreComprador: "Pedro Martínez",
                EmailComprado: "pedro.martinez@email.com"
            ));

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*No hay suficientes entradas disponibles*");
        }

        /// <summary>
        /// RF-01 → RF-02 → RF-04: Confirmar múltiples reservas del mismo evento.
        /// </summary>
        [Fact]
        public async Task FlujoCompleto_ConfirmarMultiplesReservas_DebeGenerarCodigosUnicos()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();
            var crearReserva = scope.ServiceProvider.GetRequiredService<ICrearReservaUserCase>();
            var confirmarReserva = scope.ServiceProvider.GetRequiredService<IConfirmarReservaUseCase>();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            // RF-01: Crear Evento
            var requestEvento = new RequestCrearEvento(
                Titulo: "Festival de Jazz Nocturno",
                Descripcion: "Festival de jazz con artistas nacionales e internacionales",
                CapacidadMaxima: 100,
                InicioEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
                IniciaHora: new TimeOnly(20, 0),
                FinEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(20)),
                FinHora: new TimeOnly(23, 30),
                Precio: 75m,
                VenueId: 1,
                TipoEventoId: 1
            );

            await crearEvento.ExecuteAsync(requestEvento);
            var evento = await context.Eventos.FirstAsync(e => e.Titulo == "Festival de Jazz Nocturno");

            // RF-02: Crear dos reservas
            await crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 2,
                NombreComprador: "Ana Rodríguez",
                EmailComprado: "ana.rodriguez@email.com"
            ));

            await crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 4,
                NombreComprador: "Luis Hernández",
                EmailComprado: "luis.hernandez@email.com"
            ));

            var reservas = await context.Reservas
                .Where(r => r.EventoId == evento.Id)
                .ToListAsync();
            reservas.Should().HaveCount(2);

            // RF-04: Confirmar ambas reservas
            var codigo1 = await confirmarReserva.ExecuteAsync(new RequestEstadoReserva(
                EstadoReservaEnum: EstadoReservaEnum.Confirmada,
                ReservaId: reservas[0].Id
            ));

            var codigo2 = await confirmarReserva.ExecuteAsync(new RequestEstadoReserva(
                EstadoReservaEnum: EstadoReservaEnum.Confirmada,
                ReservaId: reservas[1].Id
            ));

            codigo1.Should().StartWith("EV-");
            codigo2.Should().StartWith("EV-");

            var reservasConfirmadas = await context.Reservas
                .Where(r => r.EventoId == evento.Id && r.EstadoReservaId == (int)EstadoReservaEnum.Confirmada)
                .CountAsync();
            reservasConfirmadas.Should().Be(2);
        }

        /// <summary>
        /// RF-04: Confirmar pago de una reserva que ya fue confirmada debe fallar.
        /// </summary>
        [Fact]
        public async Task FlujoCompleto_ConfirmarReservaYaConfirmada_DebeLanzarExcepcion()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();
            var crearReserva = scope.ServiceProvider.GetRequiredService<ICrearReservaUserCase>();
            var confirmarReserva = scope.ServiceProvider.GetRequiredService<IConfirmarReservaUseCase>();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            // RF-01: Crear Evento
            await crearEvento.ExecuteAsync(new RequestCrearEvento(
                Titulo: "Conferencia de Tecnología Avanzada",
                Descripcion: "Conferencia sobre las últimas tendencias en tecnología y desarrollo",
                CapacidadMaxima: 300,
                InicioEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(45)),
                IniciaHora: new TimeOnly(10, 0),
                FinEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(45)),
                FinHora: new TimeOnly(18, 0),
                Precio: 120m,
                VenueId: 1,
                TipoEventoId: 1
            ));

            var evento = await context.Eventos.FirstAsync(e => e.Titulo == "Conferencia de Tecnología Avanzada");

            // RF-02: Reservar
            await crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 2,
                NombreComprador: "Roberto Silva",
                EmailComprado: "roberto.silva@email.com"
            ));

            var reserva = await context.Reservas.FirstAsync(r => r.EventoId == evento.Id);

            // RF-04: Confirmar primera vez (OK)
            await confirmarReserva.ExecuteAsync(new RequestEstadoReserva(
                EstadoReservaEnum: EstadoReservaEnum.Confirmada,
                ReservaId: reserva.Id
            ));

            // RF-04: Confirmar segunda vez (debe fallar)
            var act = () => confirmarReserva.ExecuteAsync(new RequestEstadoReserva(
                EstadoReservaEnum: EstadoReservaEnum.Confirmada,
                ReservaId: reserva.Id
            ));

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*ya está confirmada*");
        }

        /// <summary>
        /// RF-01 → RF-02: Reservar en un evento que no existe debe fallar.
        /// </summary>
        [Fact]
        public async Task Flujo_ReservarEnEventoInexistente_DebeLanzarExcepcion()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearReserva = scope.ServiceProvider.GetRequiredService<ICrearReservaUserCase>();

            var act = () => crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: 999,
                Cantidad: 2,
                NombreComprador: "Test Usuario",
                EmailComprado: "test@email.com"
            ));

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*No se encontró evento*");
        }

        /// <summary>
        /// RF-01 → RF-02 → RF-04: Flujo con evento de precio alto (>$100)
        /// valida máximo 10 entradas por transacción.
        /// </summary>
        [Fact]
        public async Task FlujoCompleto_EventoPrecioAlto_MaximoEntradas_DebeRespetarLimite()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();
            var crearReserva = scope.ServiceProvider.GetRequiredService<ICrearReservaUserCase>();
            var confirmarReserva = scope.ServiceProvider.GetRequiredService<IConfirmarReservaUseCase>();
            var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

            // RF-01: Crear Evento con precio > $100
            await crearEvento.ExecuteAsync(new RequestCrearEvento(
                Titulo: "Gala Premium VIP Experience",
                Descripcion: "Evento exclusivo premium con experiencia VIP para asistentes selectos",
                CapacidadMaxima: 200,
                InicioEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(60)),
                IniciaHora: new TimeOnly(20, 0),
                FinEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(60)),
                FinHora: new TimeOnly(23, 59),
                Precio: 150m,
                VenueId: 1,
                TipoEventoId: 1
            ));

            var evento = await context.Eventos.FirstAsync(e => e.Titulo == "Gala Premium VIP Experience");

            // RF-02: Intentar reservar 11 entradas (excede límite para precio > $100)
            var actExcede = () => crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 11,
                NombreComprador: "Comprador Premium Uno",
                EmailComprado: "premium.uno@email.com"
            ));

            await actExcede.Should().ThrowAsync<Exception>()
                .WithMessage("*máximo 10 entradas*");

            // RF-02: Reservar 10 entradas (dentro del límite)
            await crearReserva.ExecuteAsync(new RequestCrearReserva(
                EventoId: evento.Id,
                Cantidad: 10,
                NombreComprador: "Comprador Premium Dos",
                EmailComprado: "premium.dos@email.com"
            ));

            var reserva = await context.Reservas.FirstAsync(r => r.EventoId == evento.Id);
            reserva.Cantidad.Should().Be(10);

            // RF-04: Confirmar pago
            var codigo = await confirmarReserva.ExecuteAsync(new RequestEstadoReserva(
                EstadoReservaEnum: EstadoReservaEnum.Confirmada,
                ReservaId: reserva.Id
            ));

            codigo.Should().StartWith("EV-");
        }

        /// <summary>
        /// RF-01: Crear evento con capacidad que excede el venue debe fallar.
        /// </summary>
        [Fact]
        public async Task Flujo_CrearEventoExcedeCapacidadVenue_DebeFallar()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();

            var act = () => crearEvento.ExecuteAsync(new RequestCrearEvento(
                Titulo: "Mega Evento que Excede la Capacidad",
                Descripcion: "Este evento intenta exceder la capacidad total del venue asignado",
                CapacidadMaxima: 1000,
                InicioEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                IniciaHora: new TimeOnly(18, 0),
                FinEvento: DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                FinHora: new TimeOnly(22, 0),
                Precio: 50m,
                VenueId: 1,
                TipoEventoId: 1
            ));

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*supera la capidad del venue*");
        }

        /// <summary>
        /// RF-01: Crear evento en fin de semana después de las 22:00 debe fallar.
        /// </summary>
        [Fact]
        public async Task Flujo_CrearEventoWeekendDespuesDe22_DebeFallar()
        {
            using var scope = _serviceProvider.CreateScope();
            var crearEvento = scope.ServiceProvider.GetRequiredService<ICrearEventoUseCase>();

            // Buscar el próximo sábado
            var hoy = DateTime.Now;
            var diasHastaSabado = ((int)DayOfWeek.Saturday - (int)hoy.DayOfWeek + 7) % 7;
            if (diasHastaSabado == 0) diasHastaSabado = 7;
            var proximoSabado = hoy.AddDays(diasHastaSabado + 7);

            var act = () => crearEvento.ExecuteAsync(new RequestCrearEvento(
                Titulo: "Fiesta Nocturna de Fin de Semana",
                Descripcion: "Una fiesta nocturna que comienza después de las diez de la noche",
                CapacidadMaxima: 100,
                InicioEvento: DateOnly.FromDateTime(proximoSabado),
                IniciaHora: new TimeOnly(22, 30),
                FinEvento: DateOnly.FromDateTime(proximoSabado.AddDays(1)),
                FinHora: new TimeOnly(3, 0),
                Precio: 60m,
                VenueId: 1,
                TipoEventoId: 1
            ));

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no pueden iniciar después de las 22:00*");
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}
