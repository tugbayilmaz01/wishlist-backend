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


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();


app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
