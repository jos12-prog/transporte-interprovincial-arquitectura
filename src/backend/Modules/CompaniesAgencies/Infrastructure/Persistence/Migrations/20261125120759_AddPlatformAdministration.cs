using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contact_name",
                schema: "companies_agencies",
                table: "companies",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_phone",
                schema: "companies_agencies",
                table: "companies",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "companies_agencies",
                table: "companies",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_id",
                schema: "companies_agencies",
                table: "companies",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "companies_agencies",
                table: "agencies",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "companies_agencies",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                schema: "companies_agencies",
                table: "agencies",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_tax_id",
                schema: "companies_agencies",
                table: "companies",
                column: "tax_id",
                unique: true,
                filter: "tax_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_companies_tax_id",
                schema: "companies_agencies",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "contact_name",
                schema: "companies_agencies",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "contact_phone",
                schema: "companies_agencies",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "companies_agencies",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "tax_id",
                schema: "companies_agencies",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address",
                schema: "companies_agencies",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "companies_agencies",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "phone",
                schema: "companies_agencies",
                table: "agencies");
        }
    }
}
