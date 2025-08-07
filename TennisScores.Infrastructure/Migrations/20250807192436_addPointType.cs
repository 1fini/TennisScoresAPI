using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TennisScores.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPointType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Points");

            migrationBuilder.AddColumn<int>(
                name: "PointType",
                table: "Points",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointType",
                table: "Points");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Points",
                type: "text",
                nullable: true);
        }
    }
}
