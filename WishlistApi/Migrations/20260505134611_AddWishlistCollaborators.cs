using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WishlistApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistCollaborators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "initial_price",
                table: "products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "last_price",
                table: "products",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_price_check_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "price_currency",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "product_url",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "wishlist_collaborators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wishlist_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_collaborators", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlist_collaborators_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wishlist_collaborators_wishlists_wishlist_id",
                        column: x => x.wishlist_id,
                        principalTable: "wishlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_collaborators_user_id",
                table: "wishlist_collaborators",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_collaborators_wishlist_id",
                table: "wishlist_collaborators",
                column: "wishlist_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wishlist_collaborators");

            migrationBuilder.DropColumn(
                name: "initial_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "last_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "last_price_check_at",
                table: "products");

            migrationBuilder.DropColumn(
                name: "price_currency",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_url",
                table: "products");
        }
    }
}
