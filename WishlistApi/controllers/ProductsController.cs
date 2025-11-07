using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistApi.Data;
using WishlistApi.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }


    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {

        if (product == null)
            return BadRequest("Product cannot be null.");

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        if (updatedProduct == null || id != updatedProduct.Id)
            return BadRequest("Invalid product data.");

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound("Product not found.");

        product.Name = updatedProduct.Name;
        product.Price = updatedProduct.Price;
        product.Description = updatedProduct.Description;
        product.ImageUrl = updatedProduct.ImageUrl;


        await _context.SaveChangesAsync();
        return Ok(product);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound("Product not found.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();


        return Ok(new { message = "Product deleted successfully", productId = id });
        
    }
}
