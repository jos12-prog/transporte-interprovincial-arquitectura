using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity_access");

            migrationBuilder.CreateTable(
                name: "login_attempt_windows",
                schema: "identity_access",
                columns: table => new
                {
                    identifier_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    window_started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    blocked_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_attempt_windows", x => x.identifier_hash);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity_access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_dni = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_agency_assignments",
                schema: "identity_access",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_agency_assignments", x => new { x.UserId, x.agency_id });
                    table.ForeignKey(
                        name: "FK_user_agency_assignments_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity_access",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_agency_assignments_company_agency",
                schema: "identity_access",
                table: "user_agency_assignments",
                columns: new[] { "company_id", "agency_id" });

            migrationBuilder.CreateIndex(
                name: "UX_users_normalized_dni",
                schema: "identity_access",
                table: "users",
                column: "normalized_dni",
                unique: true,
                filter: "normalized_dni IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_users_normalized_email",
                schema: "identity_access",
                table: "users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_attempt_windows",
                schema: "identity_access");

            migrationBuilder.DropTable(
                name: "user_agency_assignments",
                schema: "identity_access");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity_access");
        }
    }
}
