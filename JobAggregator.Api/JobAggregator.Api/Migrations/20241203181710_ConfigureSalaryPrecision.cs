using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAggregator.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureSalaryPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostings_Companies_CompanyID",
                table: "JobPostings");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyID",
                table: "JobPostings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostings_Companies_CompanyID",
                table: "JobPostings",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostings_Companies_CompanyID",
                table: "JobPostings");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyID",
                table: "JobPostings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostings_Companies_CompanyID",
                table: "JobPostings",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
