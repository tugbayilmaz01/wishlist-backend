using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishlistApi.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public required string Email { get; set; }

        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("avatar")]
        public string? Avatar { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("password_reset_token")]
        public string? PasswordResetToken { get; set; }

        [Column("password_reset_token_expiry")]
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public ICollection<Wishlist>? Wishlists { get; set; }
        public ICollection<Product>? Products { get; set; }
        public ICollection<WishlistCollaborator>? SharedWishlists { get; set; }
    }
}

