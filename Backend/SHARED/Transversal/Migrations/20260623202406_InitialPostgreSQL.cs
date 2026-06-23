using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Transversal.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadoEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoEvento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadoReserva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoReserva", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoEvento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    Ciudad = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    CapacidadMaxima = table.Column<int>(type: "integer", nullable: false),
                    InicioEvento = table.Column<DateOnly>(type: "date", nullable: false),
                    IniciaHora = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    FinEvento = table.Column<DateOnly>(type: "date", nullable: false),
                    FinHora = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric", nullable: false),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    TipoEventoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_EstadoEvento_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadoEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_TipoEvento_TipoEventoId",
                        column: x => x.TipoEventoId,
                        principalTable: "TipoEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    NombreComprador = table.Column<string>(type: "text", nullable: false),
                    EmailComprador = table.Column<string>(type: "text", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EsPerdida = table.Column<bool>(type: "boolean", nullable: false),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoReservaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reserva_EstadoReserva_EstadoReservaId",
                        column: x => x.EstadoReservaId,
                        principalTable: "EstadoReserva",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reserva_Events_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstadoEvento_Id",
                table: "EstadoEvento",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoEvento_Nombre",
                table: "EstadoEvento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoReserva_Id",
                table: "EstadoReserva",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoReserva_Nombre",
                table: "EstadoReserva",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EstadoId",
                table: "Events",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Id",
                table: "Events",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_TipoEventoId",
                table: "Events",
                column: "TipoEventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Titulo",
                table: "Events",
                column: "Titulo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_EmailComprador",
                table: "Reserva",
                column: "EmailComprador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_EstadoReservaId",
                table: "Reserva",
                column: "EstadoReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_EventoId",
                table: "Reserva",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_Id",
                table: "Reserva",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_NombreComprador",
                table: "Reserva",
                column: "NombreComprador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoEvento_Id",
                table: "TipoEvento",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoEvento_Nombre",
                table: "TipoEvento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_RolId",
                table: "Usuario",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_Id",
                table: "Venues",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_Nombre",
                table: "Venues",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "EstadoReserva");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "EstadoEvento");

            migrationBuilder.DropTable(
                name: "TipoEvento");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
