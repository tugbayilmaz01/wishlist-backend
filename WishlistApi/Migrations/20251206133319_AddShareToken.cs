using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WishlistApi.Migrations
{
    /// <inheritdoc />
    public partial class AddShareToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "share_token",
                table: "wishlists",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "share_token",
                table: "wishlists");
        }
    }
}
