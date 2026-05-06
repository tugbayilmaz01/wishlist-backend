using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishlistApi.Models
{
    [Table("wishlist_collaborators")]
    public class WishlistCollaborator
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("wishlist_id")]
        public int WishlistId { get; set; }

        public Wishlist? Wishlist { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public User? User { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
