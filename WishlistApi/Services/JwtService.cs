using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class JwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userId, string email)
    {
        var claims = new[]
 {
    new Claim(ClaimTypes.NameIdentifier, userId),
    new Claim(JwtRegisteredClaimNames.Email, email)
};


        var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)
  );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiresInHours"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
