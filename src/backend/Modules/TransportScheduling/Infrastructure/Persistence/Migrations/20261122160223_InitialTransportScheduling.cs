using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTransportScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "transport_scheduling");

            migrationBuilder.CreateTable(
                name: "routes",
                schema: "transport_scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    destination_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                schema: "transport_scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    seat_layout = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_models", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "transport_scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicles_vehicle_models_vehicle_model_id",
                        column: x => x.vehicle_model_id,
                        principalSchema: "transport_scheduling",
                        principalTable: "vehicle_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                schema: "transport_scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departure_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    arrival_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.id);
                    table.ForeignKey(
                        name: "FK_trips_routes_route_id",
                        column: x => x.route_id,
                        principalSchema: "transport_scheduling",
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trips_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "transport_scheduling",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routes_origin_city_destination_city",
                schema: "transport_scheduling",
                table: "routes",
                columns: new[] { "origin_city", "destination_city" });

            migrationBuilder.CreateIndex(
                name: "IX_trips_company_id_agency_id_departure_at_utc",
                schema: "transport_scheduling",
                table: "trips",
                columns: new[] { "company_id", "agency_id", "departure_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_trips_route_id_departure_at_utc",
                schema: "transport_scheduling",
                table: "trips",
                columns: new[] { "route_id", "departure_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_trips_vehicle_id",
                schema: "transport_scheduling",
                table: "trips",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_name",
                schema: "transport_scheduling",
                table: "vehicle_models",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_company_id_agency_id",
                schema: "transport_scheduling",
                table: "vehicles",
                columns: new[] { "company_id", "agency_id" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_company_id_plate",
                schema: "transport_scheduling",
                table: "vehicles",
                columns: new[] { "company_id", "plate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_vehicle_model_id",
                schema: "transport_scheduling",
                table: "vehicles",
                column: "vehicle_model_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trips",
                schema: "transport_scheduling");

            migrationBuilder.DropTable(
                name: "routes",
                schema: "transport_scheduling");

            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "transport_scheduling");

            migrationBuilder.DropTable(
                name: "vehicle_models",
                schema: "transport_scheduling");
        }
    }
}
