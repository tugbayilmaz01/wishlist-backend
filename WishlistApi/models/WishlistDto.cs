
public class WishlistDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ShareToken { get; set; }
    public bool IsOwner { get; set; }
    public CollaboratorDto? Owner { get; set; }
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    public List<CollaboratorDto> Collaborators { get; set; } = new List<CollaboratorDto>();
}

public class CollaboratorDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; }
    public string? Avatar { get; set; }
}


public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public string? PlannedMonth { get; set; }
    public string? Category { get; set; }
}
