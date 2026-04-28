namespace WishlistApi.Models
{
    public class UserRegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class UserLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class UserUpdateDto
    {
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }
}
