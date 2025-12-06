using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WishlistApi.Data;
using WishlistApi.Models;
using System.IdentityModel.Tokens.Jwt;

namespace WishlistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }


        [HttpGet]
        public async Task<IActionResult> GetWishlists()
        {
            var userId = GetUserId();
            var wishlists = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.WishlistProducts)
                    .ThenInclude(wp => wp.Product)
                .ToListAsync();

            var result = wishlists.Select(w => new WishlistDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                ShareToken = w.ShareToken,
                Products = w.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl,
                    PlannedMonth = wp.Product.PlannedMonth
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWishlist([FromBody] Wishlist wishlist)
        {
            if (wishlist == null)
                return BadRequest("Wishlist cannot be null.");

            var userId = GetUserId();
            wishlist.UserId = userId;

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWishlists), new { id = wishlist.Id }, wishlist);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWishlist(int id)
        {
            var userId = GetUserId();
            var wishlist = await _context.Wishlists
                .Where(w => w.Id == id && w.UserId == userId)
                .Include(w => w.WishlistProducts)
                    .ThenInclude(wp => wp.Product)
                .FirstOrDefaultAsync();

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to view it.");

            var result = new WishlistDto
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                Description = wishlist.Description,
                ShareToken = wishlist.ShareToken,
                Products = wishlist.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl,
                    PlannedMonth = wp.Product.PlannedMonth
                }).ToList()
            };

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWishlist(int id)
        {
            var userId = GetUserId();
            var wishlist = await _context.Wishlists
                .Where(w => w.Id == id && w.UserId == userId)
                .Include(w => w.WishlistProducts)
                .FirstOrDefaultAsync();

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to delete it.");

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

            var userId = GetUserId();

            var wishlist = await _context.Wishlists
                .Where(w => w.Id == wishlistId && w.UserId == userId)
                .FirstOrDefaultAsync();

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to modify it.");


            product.UserId = userId;

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

            var userId = GetUserId();


            var wishlist = await _context.Wishlists
                .Where(w => w.Id == wishlistId && w.UserId == userId)
                .FirstOrDefaultAsync();

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to modify it.");

            var wishlistProduct = await _context.WishlistProducts
                .Include(wp => wp.Product)
                .Where(wp => wp.WishlistId == wishlistId && wp.ProductId == productId && wp.Product.UserId == userId)
                .FirstOrDefaultAsync();

            if (wishlistProduct == null)
                return NotFound("Product not found in this wishlist or you don't have permission to modify it.");

            wishlistProduct.Product.Name = updatedProduct.Name;
            wishlistProduct.Product.Description = updatedProduct.Description;
            wishlistProduct.Product.Price = updatedProduct.Price;
            wishlistProduct.Product.ImageUrl = updatedProduct.ImageUrl;
            wishlistProduct.Product.PlannedMonth = updatedProduct.PlannedMonth;

            await _context.SaveChangesAsync();

            return Ok(wishlistProduct.Product);
        }

        [HttpDelete("{wishlistId}/products/{productId}")]
        public async Task<IActionResult> DeleteProductFromWishlist(int wishlistId, int productId)
        {
            var userId = GetUserId();


            var wishlist = await _context.Wishlists
                .Where(w => w.Id == wishlistId && w.UserId == userId)
                .FirstOrDefaultAsync();

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to modify it.");

            var wishlistProduct = await _context.WishlistProducts
                .Where(wp => wp.WishlistId == wishlistId && wp.ProductId == productId)
                .FirstOrDefaultAsync();

            if (wishlistProduct == null)
                return NotFound("Product not found in this wishlist.");

            _context.WishlistProducts.Remove(wishlistProduct);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed from wishlist", productId });
        }
        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareWishlist(int id)
        {
            var userId = GetUserId();
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to share it.");

            if (string.IsNullOrEmpty(wishlist.ShareToken))
            {
                wishlist.ShareToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            return Ok(new { token = wishlist.ShareToken });
        }

        [HttpGet("shared/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSharedWishlist(string token)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistProducts)
                .ThenInclude(wp => wp.Product)
                .FirstOrDefaultAsync(w => w.ShareToken == token);

            if (wishlist == null)
                return NotFound("Shared wishlist not found.");

            var result = new WishlistDto
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                Description = wishlist.Description,
                ShareToken = wishlist.ShareToken,
                Products = wishlist.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl,
                    PlannedMonth = wp.Product.PlannedMonth
                }).ToList()
            };

            return Ok(result);
        }
    }
}
