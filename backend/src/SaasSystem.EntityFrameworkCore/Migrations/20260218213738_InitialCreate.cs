using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "SaasCustomers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaasTenantProfiles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Edition = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasTenantProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaasProjects",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaasProjects_SaasCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "SaasCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaasInvoices",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaasInvoices_SaasCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "SaasCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaasInvoices_SaasProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "SaasProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SaasTasks",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpentHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaasTasks_SaasProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "SaasProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaasPayments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaasPayments_SaasInvoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "dbo",
                        principalTable: "SaasInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaasCustomers_TenantId",
                schema: "dbo",
                table: "SaasCustomers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasCustomers_TenantId_Email",
                schema: "dbo",
                table: "SaasCustomers",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SaasInvoices_CustomerId",
                schema: "dbo",
                table: "SaasInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasInvoices_InvoiceNumber",
                schema: "dbo",
                table: "SaasInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaasInvoices_ProjectId",
                schema: "dbo",
                table: "SaasInvoices",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasInvoices_TenantId",
                schema: "dbo",
                table: "SaasInvoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasPayments_InvoiceId",
                schema: "dbo",
                table: "SaasPayments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasPayments_TenantId",
                schema: "dbo",
                table: "SaasPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasProjects_CustomerId",
                schema: "dbo",
                table: "SaasProjects",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasProjects_TenantId",
                schema: "dbo",
                table: "SaasProjects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasTasks_ProjectId",
                schema: "dbo",
                table: "SaasTasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasTasks_ProjectId_Status_SortOrder",
                schema: "dbo",
                table: "SaasTasks",
                columns: new[] { "ProjectId", "Status", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SaasTasks_TenantId",
                schema: "dbo",
                table: "SaasTasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasTenantProfiles_TenantId",
                schema: "dbo",
                table: "SaasTenantProfiles",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaasPayments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SaasTasks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SaasTenantProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SaasInvoices",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SaasProjects",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SaasCustomers",
                schema: "dbo");
        }
    }
}
