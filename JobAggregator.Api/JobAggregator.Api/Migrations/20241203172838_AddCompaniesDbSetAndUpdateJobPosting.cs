using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAggregator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompaniesDbSetAndUpdateJobPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyLogoUrl",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "PostedDate",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "JobPostings");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "JobPostings",
                newName: "Currency");

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "JobPostings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryFrom",
                table: "JobPostings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryTo",
                table: "JobPostings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FoundedYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headquarters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfEmployees = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_CompanyID",
                table: "JobPostings",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostings_Companies_CompanyID",
                table: "JobPostings",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostings_Companies_CompanyID",
                table: "JobPostings");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_JobPostings_CompanyID",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SalaryFrom",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SalaryTo",
                table: "JobPostings");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "JobPostings",
                newName: "CompanyName");

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoUrl",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedDate",
                table: "JobPostings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Salary",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
