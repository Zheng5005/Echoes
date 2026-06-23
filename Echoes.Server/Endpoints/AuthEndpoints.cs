using Echoes.Server.DTOs;
using Echoes.Server.Mappers;
using Echoes.Server.Models;
using Echoes.Server.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Echoes.Server.Endpoints;

public static class AuthEndpoints
{
    private const string CookieName = "jwt";
    private const string UsersCollection = "users";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/signup", SignupAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/logout", LogoutAsync);

        group.MapPut("/update-profile", UpdateProfileAsync)
            .AddEndpointFilter<AuthFilter>();

        group.MapGet("/check", CheckAuthAsync)
            .AddEndpointFilter<AuthFilter>();

        return app;
    }

    private static async Task<IResult> SignupAsync(
        SignupRequest request,
        MongoDbService mongoDb,
        JwtService jwtService,
        HttpContext httpContext,
        ILogger<AuthEndpointsLogger> logger)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new ErrorResponse("All fields are required."));
        }

        if (request.Password.Length < 6)
        {
            return Results.BadRequest(new ErrorResponse("Password must be at least 6 characters."));
        }

        var users = mongoDb.GetCollection<User>(UsersCollection);
        var existing = await users
            .Find(Builders<User>.Filter.Eq(u => u.Email, request.Email))
            .FirstOrDefaultAsync();

        if (existing is not null)
        {
            return Results.BadRequest(new ErrorResponse("Email already in use."));
        }

        var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, 10);

        var newUser = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            Password = hashedPassword,
            ProfilePic = string.Empty
        };

        await users.InsertOneAsync(newUser);

        SetJwtCookie(httpContext, jwtService, newUser.Id);

        var response = new AuthResponse("Account created", newUser.ToResponse());
        return Results.Created($"/api/auth/check", response);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        MongoDbService mongoDb,
        JwtService jwtService,
        HttpContext httpContext)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new ErrorResponse("Email and password are required."));
        }

        var users = mongoDb.GetCollection<User>(UsersCollection);
        var user = await users
            .Find(Builders<User>.Filter.Eq(u => u.Email, request.Email))
            .FirstOrDefaultAsync();

        if (user is null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
        {
            return Results.Json(new ErrorResponse("Invalid credentials"), statusCode: 401);
        }

        SetJwtCookie(httpContext, jwtService, user.Id);

        return Results.Ok(new AuthResponse("Logged in", user.ToResponse()));
    }

    private static IResult LogoutAsync(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = httpContext.Request.IsHttps,
            Path = "/"
        });
        return Results.Ok(new { message = "Logged out" });
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateProfileRequest request,
        CloudinaryService cloudinary,
        MongoDbService mongoDb,
        HttpContext httpContext)
    {
        if (string.IsNullOrWhiteSpace(request.ProfilePic))
        {
            return Results.BadRequest(new ErrorResponse("Profile picture is required."));
        }

        var currentUser = (User)httpContext.Items[AuthFilter.UserItemKey]!;

        var secureUrl = await cloudinary.UploadImageAsync(request.ProfilePic);

        var users = mongoDb.GetCollection<User>(UsersCollection);
        var update = Builders<User>.Update
            .Set(u => u.ProfilePic, secureUrl)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After };

        var updated = await users.FindOneAndUpdateAsync(
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(currentUser.Id)),
            update,
            options);

        if (updated is null)
        {
            return Results.NotFound(new ErrorResponse("User not found."));
        }

        return Results.Ok(new CheckAuthResponse(updated.ToResponse()));
    }

    private static IResult CheckAuthAsync(HttpContext httpContext)
    {
        var currentUser = (User)httpContext.Items[AuthFilter.UserItemKey]!;
        return Results.Ok(new CheckAuthResponse(currentUser.ToResponse()));
    }

    private static void SetJwtCookie(HttpContext httpContext, JwtService jwtService, string userId)
    {
        var token = jwtService.GenerateToken(userId);

        httpContext.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = httpContext.Request.IsHttps,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            MaxAge = TimeSpan.FromDays(7)
        });
    }
}

/// <summary>
/// Marker logger category for static auth endpoint handlers.
/// </summary>
public sealed class AuthEndpointsLogger { }
