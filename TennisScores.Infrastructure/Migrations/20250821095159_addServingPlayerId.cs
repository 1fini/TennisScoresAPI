using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TennisScores.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addServingPlayerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServingPlayerId",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServingPlayerId",
                table: "Matches");
        }
    }
}
