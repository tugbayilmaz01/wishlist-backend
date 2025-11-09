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
        public string Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("imageurl")]
        public string ImageUrl { get; set; }

        [Column("month")]
        
        public string PlannedMonth { get; set; }

    }
}
