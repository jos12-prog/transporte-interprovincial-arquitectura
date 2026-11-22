using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCompaniesAgencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "companies_agencies");

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "companies_agencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agencies",
                schema: "companies_agencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agencies", x => x.id);
                    table.ForeignKey(
                        name: "FK_agencies_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "companies_agencies",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agencies_company_id_city",
                schema: "companies_agencies",
                table: "agencies",
                columns: new[] { "company_id", "city" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agencies",
                schema: "companies_agencies");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "companies_agencies");
        }
    }
}
