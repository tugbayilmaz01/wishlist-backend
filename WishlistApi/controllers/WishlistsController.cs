using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistApi.Data;
using WishlistApi.Models;
using System.Threading.Tasks;
using System.Linq;


namespace WishlistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlists()
        {
            var wishlists = await _context.Wishlists
                .Include(w => w.WishlistProducts)
                    .ThenInclude(wp => wp.Product)
                .ToListAsync();

            var result = wishlists.Select(w => new WishlistDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                Products = w.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost("{wishlistId}/products")]
        public async Task<IActionResult> AddProductToWishlist(int wishlistId, [FromBody] Product product)
        {
            if (product == null) return BadRequest("Product cannot be null.");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var wishlistProduct = new WishlistProduct
            {
                WishlistId = wishlistId,
                ProductId = product.Id
            };

            _context.WishlistProducts.Add(wishlistProduct);
            await _context.SaveChangesAsync();

            return Ok(new { product, wishlistProduct });
        }
    }
}
