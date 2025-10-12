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
}
