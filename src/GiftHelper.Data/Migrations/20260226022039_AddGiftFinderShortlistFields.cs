using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftFinderShortlistFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedMaxPrice",
                table: "GiftIdeas",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedMinPrice",
                table: "GiftIdeas",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedId",
                table: "GiftIdeas",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "GiftIdeas",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiftIdeas_SeedId",
                table: "GiftIdeas",
                column: "SeedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GiftIdeas_SeedId",
                table: "GiftIdeas");

            migrationBuilder.DropColumn(
                name: "EstimatedMaxPrice",
                table: "GiftIdeas");

            migrationBuilder.DropColumn(
                name: "EstimatedMinPrice",
                table: "GiftIdeas");

            migrationBuilder.DropColumn(
                name: "SeedId",
                table: "GiftIdeas");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "GiftIdeas");
        }
    }
}
