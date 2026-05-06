using System.ComponentModel.DataAnnotations.Schema;

namespace WishlistApi.Models
{
    [Table("products")]
    public class Product
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("imageurl")]
        [System.Text.Json.Serialization.JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [Column("product_url")]
        [System.Text.Json.Serialization.JsonPropertyName("productUrl")]
        public string? ProductUrl { get; set; }

        [Column("month")]
        [System.Text.Json.Serialization.JsonPropertyName("plannedMonth")]
        public string? PlannedMonth { get; set; }

        [Column("category")]
        [System.Text.Json.Serialization.JsonPropertyName("category")]
        public string? Category { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
