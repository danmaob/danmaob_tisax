using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanmaobTisax.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantModuleExceptionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "TenantModules",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_ModuleCode",
                table: "TenantModules",
                column: "ModuleCode");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantModules_FunctionalModules_ModuleCode",
                table: "TenantModules",
                column: "ModuleCode",
                principalTable: "FunctionalModules",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantModules_FunctionalModules_ModuleCode",
                table: "TenantModules");

            migrationBuilder.DropIndex(
                name: "IX_TenantModules_ModuleCode",
                table: "TenantModules");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "TenantModules",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }
    }
}
