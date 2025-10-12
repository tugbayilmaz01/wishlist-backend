using DotNetEnv; 
using Microsoft.EntityFrameworkCore;
using WishlistApi.Data;

Env.Load(); 

var builder = WebApplication.CreateBuilder(args);


var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");


var connectionString = $"{baseConnectionString}Password={dbPassword};";


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
