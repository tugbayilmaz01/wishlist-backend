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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<WishlistProduct>()
                .HasOne(wp => wp.Wishlist)
                .WithMany(w => w.WishlistProducts)
                .HasForeignKey(wp => wp.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<WishlistProduct>()
                .HasOne(wp => wp.Product)
                .WithMany()
                .HasForeignKey(wp => wp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
