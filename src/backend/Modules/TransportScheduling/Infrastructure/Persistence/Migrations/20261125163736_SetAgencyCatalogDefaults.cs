using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetAgencyCatalogDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Scheduled",
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "routes",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "fares",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "transport_scheduling",
                table: "trips",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldDefaultValue: "Scheduled");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "routes",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "transport_scheduling",
                table: "fares",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
