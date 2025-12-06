
public class WishlistDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? ShareToken { get; set; }
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
}


public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public string? PlannedMonth { get; set; }
}
