using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reservations");

            migrationBuilder.CreateTable(
                name: "trip_seats",
                schema: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<int>(type: "integer", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: true),
                    column_number = table.Column<int>(type: "integer", nullable: true),
                    floor = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_seats", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trip_seats_company_id_agency_id_trip_id",
                schema: "reservations",
                table: "trip_seats",
                columns: new[] { "company_id", "agency_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "IX_trip_seats_trip_id_seat_number",
                schema: "reservations",
                table: "trip_seats",
                columns: new[] { "trip_id", "seat_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trip_seats_trip_id_status",
                schema: "reservations",
                table: "trip_seats",
                columns: new[] { "trip_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trip_seats",
                schema: "reservations");
        }
    }
}
