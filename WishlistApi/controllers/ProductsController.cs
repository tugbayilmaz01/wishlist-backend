using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WishlistApi.Data;
using WishlistApi.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
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
    public async Task<IActionResult> GetProducts()
    {
        var userId = GetUserId();
        var products = await _context.Products
            .Where(p => p.UserId == userId)
            .ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        if (product == null)
            return BadRequest("Product cannot be null.");

        var userId = GetUserId();
        product.UserId = userId;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        if (updatedProduct == null || id != updatedProduct.Id)
            return BadRequest("Invalid product data.");

        var userId = GetUserId();
        var product = await _context.Products
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound("Product not found or you don't have permission to update it.");

        product.Name = updatedProduct.Name;
        product.Price = updatedProduct.Price;
        product.Description = updatedProduct.Description;
        product.ImageUrl = updatedProduct.ImageUrl;
        product.PlannedMonth = updatedProduct.PlannedMonth;

        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var userId = GetUserId();
        var product = await _context.Products
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound("Product not found or you don't have permission to delete it.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully", productId = id });
    }
}
