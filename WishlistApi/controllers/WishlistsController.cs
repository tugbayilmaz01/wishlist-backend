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
                .Where(w => w.UserId == userId || w.WishlistCollaborators.Any(wc => wc.UserId == userId))
                .Include(w => w.User)
                .Include(w => w.WishlistProducts)
                    .ThenInclude(wp => wp.Product)
                .Include(w => w.WishlistCollaborators)
                    .ThenInclude(wc => wc.User)
                .ToListAsync();

            var result = wishlists.Select(w => new WishlistDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                ShareToken = w.ShareToken,
                IsOwner = w.UserId == userId,
                Owner = w.User != null ? new CollaboratorDto
                {
                    Id = w.User.Id,
                    Name = w.User.Name,
                    Email = w.User.Email,
                    Avatar = w.User.Avatar
                } : null,
                Products = w.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl,
                    PlannedMonth = wp.Product.PlannedMonth,
                    Category = wp.Product.Category
                }).ToList(),
                Collaborators = w.WishlistCollaborators.Select(wc => new CollaboratorDto
                {
                    Id = wc.User.Id,
                    Name = wc.User.Name,
                    Email = wc.User.Email,
                    Avatar = wc.User.Avatar
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
                .Where(w => w.Id == id && (w.UserId == userId || w.WishlistCollaborators.Any(wc => wc.UserId == userId)))
                .Include(w => w.User)
                .Include(w => w.WishlistProducts)
                    .ThenInclude(wp => wp.Product)
                .Include(w => w.WishlistCollaborators)
                    .ThenInclude(wc => wc.User)
                .FirstOrDefaultAsync();

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to view it.");

            var result = new WishlistDto
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                Description = wishlist.Description,
                ShareToken = wishlist.ShareToken,
                IsOwner = wishlist.UserId == userId,
                Owner = wishlist.User != null ? new CollaboratorDto
                {
                    Id = wishlist.User.Id,
                    Name = wishlist.User.Name,
                    Email = wishlist.User.Email,
                    Avatar = wishlist.User.Avatar
                } : null,
                Products = wishlist.WishlistProducts.Select(wp => new ProductDto
                {
                    Id = wp.Product.Id,
                    Name = wp.Product.Name,
                    Description = wp.Product.Description,
                    Price = wp.Product.Price,
                    ImageUrl = wp.Product.ImageUrl,
                    PlannedMonth = wp.Product.PlannedMonth,
                    Category = wp.Product.Category
                }).ToList(),
                Collaborators = wishlist.WishlistCollaborators.Select(wc => new CollaboratorDto
                {
                    Id = wc.User.Id,
                    Name = wc.User.Name,
                    Email = wc.User.Email,
                    Avatar = wc.User.Avatar
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWishlist(int id, [FromBody] Wishlist updatedWishlist)
        {
            if (updatedWishlist == null || id != updatedWishlist.Id)
                return BadRequest("Invalid wishlist data.");

            var userId = GetUserId();
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to modify it.");

            wishlist.Name = updatedWishlist.Name;
            wishlist.Description = updatedWishlist.Description;

            await _context.SaveChangesAsync();

            return Ok(wishlist);
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
                .Where(w => w.Id == wishlistId && (w.UserId == userId || w.WishlistCollaborators.Any(wc => wc.UserId == userId)))
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

            return Ok(new
            {
                product = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    PlannedMonth = product.PlannedMonth,
                    Category = product.Category
                }
            });
        }

        [HttpPut("{wishlistId}/products/{productId}")]
        public async Task<IActionResult> UpdateProductInWishlist(int wishlistId, int productId, [FromBody] Product updatedProduct)
        {
            if (updatedProduct == null || productId != updatedProduct.Id)
                return BadRequest("Invalid product data.");

            var userId = GetUserId();


            var wishlist = await _context.Wishlists
                .Where(w => w.Id == wishlistId && (w.UserId == userId || w.WishlistCollaborators.Any(wc => wc.UserId == userId)))
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
            wishlistProduct.Product.Category = updatedProduct.Category;

            await _context.SaveChangesAsync();

            return Ok(wishlistProduct.Product);
        }

        [HttpDelete("{wishlistId}/products/{productId}")]
        public async Task<IActionResult> DeleteProductFromWishlist(int wishlistId, int productId)
        {
            var userId = GetUserId();


            var wishlist = await _context.Wishlists
                .Where(w => w.Id == wishlistId && (w.UserId == userId || w.WishlistCollaborators.Any(wc => wc.UserId == userId)))
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
                    PlannedMonth = wp.Product.PlannedMonth,
                    Category = wp.Product.Category
                }).ToList()
            };

            return Ok(result);
        }

        public class InviteRequest
        {
            public string Email { get; set; }
        }

        [HttpPost("{id}/collaborators")]
        public async Task<IActionResult> AddCollaborator(int id, [FromBody] InviteRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var userId = GetUserId();
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistCollaborators)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to add collaborators.");

            var userToAdd = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (userToAdd == null)
                return NotFound("User with this email not found.");

            if (userToAdd.Id == userId)
                return BadRequest("You cannot add yourself as a collaborator.");

            if (wishlist.WishlistCollaborators.Any(wc => wc.UserId == userToAdd.Id))
                return BadRequest("User is already a collaborator.");

            var collaborator = new WishlistCollaborator
            {
                WishlistId = id,
                UserId = userToAdd.Id
            };

            _context.WishlistCollaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            return Ok(new CollaboratorDto
            {
                Id = userToAdd.Id,
                Name = userToAdd.Name,
                Email = userToAdd.Email,
                Avatar = userToAdd.Avatar
            });
        }

        [HttpDelete("{id}/collaborators/{collaboratorId}")]
        public async Task<IActionResult> RemoveCollaborator(int id, int collaboratorId)
        {
            var userId = GetUserId();
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistCollaborators)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (wishlist == null)
                return NotFound("Wishlist not found or you don't have permission to modify collaborators.");

            var collaborator = wishlist.WishlistCollaborators.FirstOrDefault(wc => wc.UserId == collaboratorId);
            if (collaborator == null)
                return NotFound("Collaborator not found.");

            _context.WishlistCollaborators.Remove(collaborator);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Collaborator removed successfully" });
        }
    }
}
