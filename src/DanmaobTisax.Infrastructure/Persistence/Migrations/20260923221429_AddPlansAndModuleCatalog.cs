using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DanmaobTisax.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlansAndModuleCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionalModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionalModules", x => x.Id);
                    table.UniqueConstraint("AK_FunctionalModules_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanModules_FunctionalModules_ModuleCode",
                        column: x => x.ModuleCode,
                        principalTable: "FunctionalModules",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanModules_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "FunctionalModules",
                columns: new[] { "Id", "Code", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("7a0b3c20-0000-4000-8000-000000000001"), "Organization", 1 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000002"), "Scope", 2 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000003"), "Catalog", 3 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000004"), "GapAnalysis", 4 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000005"), "Risk", 5 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000006"), "Documents", 6 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000007"), "Evidence", 7 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000008"), "Capa", 8 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000009"), "Audits", 9 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000010"), "Readiness", 10 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000011"), "Dashboard", 11 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000012"), "Reports", 12 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000013"), "ThirdParties", 13 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000014"), "Assets", 14 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000015"), "PhysicalPrototype", 15 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000016"), "UsersAccess", 16 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000017"), "Notifications", 17 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000018"), "LabelLifecycle", 18 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000019"), "KnowledgeBase", 19 },
                    { new Guid("7a0b3c20-0000-4000-8000-000000000020"), "Administration", 20 }
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("7a0b3c10-0000-4000-8000-000000000001"), "FREE", true, "Free" },
                    { new Guid("7a0b3c10-0000-4000-8000-000000000002"), "BASIC", true, "Básico" },
                    { new Guid("7a0b3c10-0000-4000-8000-000000000003"), "PREMIUM", true, "Premium" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanModule_PlanId_ModuleCode",
                table: "PlanModules",
                columns: new[] { "PlanId", "ModuleCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanModules_ModuleCode",
                table: "PlanModules",
                column: "ModuleCode");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_Code",
                table: "Plans",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanModules");

            migrationBuilder.DropTable(
                name: "FunctionalModules");

            migrationBuilder.DropTable(
                name: "Plans");
        }
    }
}
