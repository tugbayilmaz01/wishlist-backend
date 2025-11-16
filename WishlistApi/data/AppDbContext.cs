using Microsoft.EntityFrameworkCore;
using WishlistApi.Models;

namespace WishlistApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<Product> Products { get; set; }


        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistProduct> WishlistProducts { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
