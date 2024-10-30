using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAggregator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyLogoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Salary",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalID",
                table: "JobPostings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoUrl",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_ExternalID",
                table: "JobPostings",
                column: "ExternalID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPostings_ExternalID",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "CompanyLogoUrl",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "Salary",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalID",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
