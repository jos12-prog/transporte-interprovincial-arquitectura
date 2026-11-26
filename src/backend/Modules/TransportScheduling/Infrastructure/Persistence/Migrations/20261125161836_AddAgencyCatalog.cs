using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "transport_scheduling",
                table: "vehicles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "destination_city_snapshot",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "origin_city_snapshot",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "vehicle_display_name_snapshot",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "vehicle_model_name_snapshot",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                schema: "transport_scheduling",
                table: "routes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "routes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "fares",
                schema: "transport_scheduling",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fares", x => x.id);
                    table.ForeignKey(
                        name: "FK_fares_routes_route_id",
                        column: x => x.route_id,
                        principalSchema: "transport_scheduling",
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routes_company_id_origin_city_destination_city",
                schema: "transport_scheduling",
                table: "routes",
                columns: new[] { "company_id", "origin_city", "destination_city" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fares_company_id_route_id_type_valid_from_valid_to",
                schema: "transport_scheduling",
                table: "fares",
                columns: new[] { "company_id", "route_id", "type", "valid_from", "valid_to" });

            migrationBuilder.CreateIndex(
                name: "IX_fares_route_id",
                schema: "transport_scheduling",
                table: "fares",
                column: "route_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fares",
                schema: "transport_scheduling");

            migrationBuilder.DropIndex(
                name: "IX_routes_company_id_origin_city_destination_city",
                schema: "transport_scheduling",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "transport_scheduling",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "transport_scheduling",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "destination_city_snapshot",
                schema: "transport_scheduling",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "origin_city_snapshot",
                schema: "transport_scheduling",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "transport_scheduling",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "vehicle_display_name_snapshot",
                schema: "transport_scheduling",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "vehicle_model_name_snapshot",
                schema: "transport_scheduling",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "company_id",
                schema: "transport_scheduling",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "transport_scheduling",
                table: "routes");

        }
    }
}
