using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs;
using RoundsApp.Models;
using RoundsApp.Services;

namespace RoundsApp.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authApi = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        authApi.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi();

        authApi.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi();
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration)
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
        var jwtSettings = configuration.GetSection("JwtSettings");

        return Results.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60"))
        });
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Unauthorized();
        }

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        var token = tokenService.GenerateToken(user);
        var jwtSettings = configuration.GetSection("JwtSettings");

        return Results.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60"))
        });
    }
}
