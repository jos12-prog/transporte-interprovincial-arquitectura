using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleModelAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                schema: "transport_scheduling",
                table: "vehicle_models",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "vehicle_models",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                schema: "transport_scheduling",
                table: "vehicle_models");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "transport_scheduling",
                table: "vehicle_models");
        }
    }
}
