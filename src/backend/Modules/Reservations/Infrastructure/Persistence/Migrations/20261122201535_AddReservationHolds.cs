using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationHolds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    hold_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservation_seats",
                schema: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_seat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_seats", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservation_seats_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalSchema: "reservations",
                        principalTable: "reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reservation_seats_reservation_id_status",
                schema: "reservations",
                table: "reservation_seats",
                columns: new[] { "reservation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "UX_reservation_seats_active_trip_seat",
                schema: "reservations",
                table: "reservation_seats",
                column: "trip_seat_id",
                unique: true,
                filter: "status IN ('Held', 'Confirmed')");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_company_id_agency_id_trip_id",
                schema: "reservations",
                table: "reservations",
                columns: new[] { "company_id", "agency_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "IX_reservations_status_expires_at_utc",
                schema: "reservations",
                table: "reservations",
                columns: new[] { "status", "expires_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_seats",
                schema: "reservations");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "reservations");
        }
    }
}
