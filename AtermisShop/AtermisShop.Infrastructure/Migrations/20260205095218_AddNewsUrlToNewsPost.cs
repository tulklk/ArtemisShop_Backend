using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtermisShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsUrlToNewsPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewsUrl",
                table: "NewsPosts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsUrl",
                table: "NewsPosts");
        }
    }
}
