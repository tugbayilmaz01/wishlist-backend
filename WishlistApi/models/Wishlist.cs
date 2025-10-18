using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishlistApi.Models
{
    [Table("wishlists")]
    public class Wishlist
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

   public ICollection<WishlistProduct>? WishlistProducts { get; set; }

    }
}
