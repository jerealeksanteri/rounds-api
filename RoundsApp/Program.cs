using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoundsApp.Data;
using RoundsApp.DTOs;
using RoundsApp.Models;
using RoundsApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Add Services
builder.Services.AddScoped<ITokenService, TokenService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Authentication Endpoints
var authApi = app.MapGroup("/api/auth");

authApi.MapPost("/register", async (RegisterRequest request, UserManager<ApplicationUser> userManager, ITokenService tokenService) =>
{
    var user = new ApplicationUser
    {
        UserName = request.UserName ?? request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName
    };

    var result = await userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    var token = tokenService.GenerateToken(user);

    return Results.Ok(new AuthResponse
    {
        Token = token,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName ?? string.Empty,
        LastName = user.LastName ?? string.Empty,
        ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60"))
    });
});

authApi.MapPost("/login", async (LoginRequest request, UserManager<ApplicationUser> userManager, ITokenService tokenService) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);

    if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
    {
        return Results.Unauthorized();
    }

    user.LastLoginAt = DateTime.UtcNow;
    await userManager.UpdateAsync(user);

    var token = tokenService.GenerateToken(user);

    return Results.Ok(new AuthResponse
    {
        Token = token,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName ?? string.Empty,
        LastName = user.LastName ?? string.Empty,
        ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60"))
    });
});

// Sample protected endpoint
var todosApi = app.MapGroup("/api/todos").RequireAuthorization();

var sampleTodos = new List<Todo>
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
