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
        public required string PasswordHash { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
