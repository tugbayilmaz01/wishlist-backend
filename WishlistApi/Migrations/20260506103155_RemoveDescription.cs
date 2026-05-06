using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WishlistApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "wishlists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "wishlists",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
