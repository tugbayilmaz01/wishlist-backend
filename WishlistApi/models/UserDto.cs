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
}
