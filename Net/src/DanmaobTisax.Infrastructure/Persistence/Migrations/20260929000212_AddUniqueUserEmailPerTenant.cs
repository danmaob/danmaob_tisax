using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanmaobTisax.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserEmailPerTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_TenantId_Email",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_User_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_TenantId_Email",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_User_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" });
        }
    }
}
