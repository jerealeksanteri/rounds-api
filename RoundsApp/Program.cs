// <copyright file="Program.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoundsApp.Data;
using RoundsApp.Endpoints;
using RoundsApp.Hubs;
using RoundsApp.Models;
using RoundsApp.Repositories;
using RoundsApp.Repositories.IRepositories;
using RoundsApp.Services;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/rounds-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Extract token and path
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // If access token is present and path directs to hubs
            var isHubsAuthenticated = !string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs");

            // If is authenticated insert token into context
            if (isHubsAuthenticated)
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IFriendGroupValidationService, FriendGroupValidationService>();
builder.Services.AddScoped<IMentionService, MentionService>();

// Add Repositories
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IDrinkRepository, DrinkRepository>();
builder.Services.AddScoped<IDrinkTypeRepository, DrinkTypeRepository>();
builder.Services.AddScoped<IDrinkImageRepository, DrinkImageRepository>();
builder.Services.AddScoped<IDrinkingSessionRepository, DrinkingSessionRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IFriendGroupRepository, FriendGroupRepository>();
builder.Services.AddScoped<IFriendGroupMemberRepository, FriendGroupMemberRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISessionAchievementRepository, SessionAchievementRepository>();
builder.Services.AddScoped<ISessionCommentRepository, SessionCommentRepository>();
builder.Services.AddScoped<ICommentMentionRepository, CommentMentionRepository>();
builder.Services.AddScoped<ISessionImageRepository, SessionImageRepository>();
builder.Services.AddScoped<ISessionInviteRepository, SessionInviteRepository>();
builder.Services.AddScoped<ISessionLocationRepository, SessionLocationRepository>();
builder.Services.AddScoped<ISessionParticipantRepository, SessionParticipantRepository>();
builder.Services.AddScoped<ISessionTagRepository, SessionTagRepository>();
builder.Services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
builder.Services.AddScoped<IUserDrinkRepository, UserDrinkRepository>();
builder.Services.AddScoped<IUserFavouriteDrinkRepository, UserFavouriteDrinkRepository>();

// Add OpenAPI
builder.Services.AddOpenApi();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

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
    await db.Database.MigrateAsync();
}

// Configure OpenAPI and Scalar
app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapAuthEndpoints();
app.MapSessionEndpoints();
app.MapSessionParticipantEndpoints();
app.MapSessionCommentEndpoints();
app.MapSessionInviteEndpoints();
app.MapSessionTagEndpoints();
app.MapSessionImageEndpoints();
app.MapSessionLocationEndpoints();
app.MapDrinkEndpoints();
app.MapDrinkTypeEndpoints();
app.MapFriendshipEndpoints();
app.MapFriendGroupEndpoints();
app.MapAchievementEndpoints();
app.MapNotificationEndpoints();

// Map Health Checks
app.MapHealthChecks("/health");

// Map Hub
app.MapHub<NotificationHub>("hubs/notifications");

await app.RunAsync();

// Make the implicit Program class public for testing
public partial class Program
{
}
