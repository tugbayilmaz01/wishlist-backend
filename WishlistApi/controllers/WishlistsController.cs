using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistApi.Data;
using WishlistApi.Models;
using System.Linq;
using System.Threading.Tasks;

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

       
        [HttpPost]
        public async Task<IActionResult> CreateWishlist([FromBody] Wishlist wishlist)
        {
            if (wishlist == null)
                return BadRequest("Wishlist cannot be null.");

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWishlists), new { id = wishlist.Id }, wishlist);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWishlist(int id)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistProducts)
                    .ThenInclude(wp => wp.Product)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wishlist == null)
                return NotFound();

            var result = new WishlistDto
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                Description = wishlist.Description,
                Products = wishlist.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl
                }).ToList()
            };

            return Ok(result);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWishlist(int id)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistProducts)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wishlist == null)
                return NotFound("Wishlist not found.");

            if (wishlist.WishlistProducts.Any())
                _context.WishlistProducts.RemoveRange(wishlist.WishlistProducts);

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Wishlist deleted successfully", wishlistId = id });
        }

    
        [HttpPost("{wishlistId}/products")]
        public async Task<IActionResult> AddProductToWishlist(int wishlistId, [FromBody] Product product)
        {
            if (product == null)
                return BadRequest("Product cannot be null.");

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

      
        [HttpPut("{wishlistId}/products/{productId}")]
        public async Task<IActionResult> UpdateProductInWishlist(int wishlistId, int productId, [FromBody] Product updatedProduct)
        {
            if (updatedProduct == null || productId != updatedProduct.Id)
                return BadRequest("Invalid product data.");

            var wishlistProduct = await _context.WishlistProducts
                .Include(wp => wp.Product)
                .FirstOrDefaultAsync(wp => wp.WishlistId == wishlistId && wp.ProductId == productId);

            if (wishlistProduct == null)
                return NotFound("Product not found in this wishlist.");

            wishlistProduct.Product.Name = updatedProduct.Name;
            wishlistProduct.Product.Description = updatedProduct.Description;
            wishlistProduct.Product.Price = updatedProduct.Price;
            wishlistProduct.Product.ImageUrl = updatedProduct.ImageUrl;

            await _context.SaveChangesAsync();

            return Ok(wishlistProduct.Product);
        }

       
        [HttpDelete("{wishlistId}/products/{productId}")]
        public async Task<IActionResult> DeleteProductFromWishlist(int wishlistId, int productId)
        {
            var wishlistProduct = await _context.WishlistProducts
                .FirstOrDefaultAsync(wp => wp.WishlistId == wishlistId && wp.ProductId == productId);

            if (wishlistProduct == null)
                return NotFound("Product not found in this wishlist.");

            _context.WishlistProducts.Remove(wishlistProduct);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed from wishlist", productId });
        }
    }
}
