using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAggregator.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPostings_ExternalID",
                table: "JobPostings");

            migrationBuilder.DropIndex(
                name: "IX_JobPostings_HashCode",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "HashCode",
                table: "JobPostings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalID",
                table: "JobPostings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_ExternalID",
                table: "JobPostings",
                column: "ExternalID",
                unique: true,
                filter: "[ExternalID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_HashCode",
                table: "JobPostings",
                column: "HashCode",
                unique: true,
                filter: "[HashCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPostings_ExternalID",
                table: "JobPostings");

            migrationBuilder.DropIndex(
                name: "IX_JobPostings_HashCode",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "HashCode",
                table: "JobPostings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalID",
                table: "JobPostings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_ExternalID",
                table: "JobPostings",
                column: "ExternalID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_HashCode",
                table: "JobPostings",
                column: "HashCode",
                unique: true);
        }
    }
}
