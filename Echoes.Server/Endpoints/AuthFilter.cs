using Echoes.Server.Models;
using Echoes.Server.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Echoes.Server.Endpoints;

/// <summary>
/// Endpoint filter that authenticates a request by reading the "jwt" cookie,
/// validating the token, and loading the user from MongoDB. The resolved
/// user is exposed via HttpContext.Items["User"] for downstream handlers.
/// </summary>
public class AuthFilter(JwtService jwtService, MongoDbService mongoDb) : IEndpointFilter
{
    public const string UserItemKey = "User";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Request.Cookies.TryGetValue("jwt", out var token) || string.IsNullOrWhiteSpace(token))
        {
            return Results.Unauthorized();
        }

        var principal = jwtService.ValidateToken(token);
        if (principal is null)
        {
            return Results.Unauthorized();
        }

        var userId = principal.FindFirst("userId")?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
        {
            return Results.Unauthorized();
        }

        var users = mongoDb.GetCollection<User>("users");
        var user = await users.Find(Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId))).FirstOrDefaultAsync();

        if (user is null)
        {
            return Results.Unauthorized();
        }

        httpContext.Items[UserItemKey] = user;
        return await next(context);
    }
}
