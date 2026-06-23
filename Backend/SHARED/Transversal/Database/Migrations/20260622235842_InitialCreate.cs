using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transversal.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadoEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoEvento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadoReserva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoReserva", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoEvento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: false),
                    InicioEvento = table.Column<DateOnly>(type: "date", nullable: false),
                    IniciaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinEvento = table.Column<DateOnly>(type: "date", nullable: false),
                    FinHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    TipoEventoId = table.Column<int>(type: "int", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false)
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
                name: "EstadoReserva");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EstadoEvento");

            migrationBuilder.DropTable(
                name: "TipoEvento");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
