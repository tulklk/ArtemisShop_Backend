using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtermisShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModel3DUrlToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "Products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "Products");
        }
    }
}
