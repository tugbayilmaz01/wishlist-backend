using System.ComponentModel.DataAnnotations.Schema;

namespace WishlistApi.Models
{
    [Table("wishlist_products")]
    public class WishlistProduct
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("wishlist_id")]
        public int WishlistId { get; set; }
        public Wishlist Wishlist { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
