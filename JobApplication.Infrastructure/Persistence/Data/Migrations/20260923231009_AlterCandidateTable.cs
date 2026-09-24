using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterCandidateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVFileName",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVPublicId",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CVUploadedAt",
                table: "Candidates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortfolioUrl",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "Candidates",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CVFileName",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CVPublicId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CVUploadedAt",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "PortfolioUrl",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "Candidates");
        }
    }
}
