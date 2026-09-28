using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanmaobTisax.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAdministrators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformAdministrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FailedLoginAttemptCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdministrators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdministrator_Email",
                table: "PlatformAdministrators",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformAdministrators");
        }
    }
}
