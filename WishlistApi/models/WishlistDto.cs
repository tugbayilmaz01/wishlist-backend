
public class WishlistDto
{
    public int Id { get; set; }
    public string Name { get; set; }
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
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("price")]
    public decimal Price { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("productUrl")]
    public string? ProductUrl { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("plannedMonth")]
    public string? PlannedMonth { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("category")]
    public string? Category { get; set; }
}
