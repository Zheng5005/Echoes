using Echoes.Server.ConnectionManager;
using Echoes.Server.DTOs;
using Echoes.Server.Hubs;
using Echoes.Server.Mappers;
using Echoes.Server.Models;
using Echoes.Server.Services;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Echoes.Server.Endpoints;

public static class MessageEndpoints
{
    private const string UsersCollection = "users";
    private const string MessagesCollection = "messages";

    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/messages")
            .AddEndpointFilter<AuthFilter>();

        group.MapGet("/users", GetUsersAsync);
        group.MapGet("/{id}", GetMessagesAsync);
        group.MapPost("/send/{id}", SendMessageAsync);

        return app;
    }

    private static async Task<IResult> GetUsersAsync(
        MongoDbService mongoDb,
        HttpContext httpContext)
    {
        var currentUser = (User)httpContext.Items[AuthFilter.UserItemKey]!;

        var users = mongoDb.GetCollection<User>(UsersCollection);

        var filter = Builders<User>.Filter.Ne("_id", ObjectId.Parse(currentUser.Id));
        var projection = Builders<User>.Projection.Exclude("password");

        var others = await users
            .Find(filter)
            .Project<User>(projection)
            .ToListAsync();

        var response = new UsersListResponse(others.Select(u => u.ToResponse()).ToList());
        return Results.Ok(response);
    }

    private static async Task<IResult> GetMessagesAsync(
        string id,
        MongoDbService mongoDb,
        HttpContext httpContext)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return Results.BadRequest(new ErrorResponse("Invalid user id."));
        }

        var currentUser = (User)httpContext.Items[AuthFilter.UserItemKey]!;
        var myId = currentUser.Id;
        var partnerId = id;

        var messages = mongoDb.GetCollection<Message>(MessagesCollection);

        var filter = Builders<Message>.Filter.Or(
            Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.SenderId, myId),
                Builders<Message>.Filter.Eq(m => m.ReceiverId, partnerId)),
            Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.SenderId, partnerId),
                Builders<Message>.Filter.Eq(m => m.ReceiverId, myId)));

        var conversation = await messages
            .Find(filter)
            .SortBy(m => m.CreatedAt)
            .ToListAsync();

        var response = new MessagesListResponse(conversation.Select(m => m.ToResponse()).ToList());
        return Results.Ok(response);
    }

    private static async Task<IResult> SendMessageAsync(
        string id,
        SendMessageRequest request,
        CloudinaryService cloudinary,
        MongoDbService mongoDb,
        UserConnectionManager connectionManager,
        IHubContext<ChatHub> hubContext,
        HttpContext httpContext,
        ILogger<MessageEndpointsLogger> logger)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return Results.BadRequest(new ErrorResponse("Invalid receiver id."));
        }

        if (string.IsNullOrWhiteSpace(request.Text) && string.IsNullOrWhiteSpace(request.Image))
        {
            return Results.BadRequest(new ErrorResponse("Message must contain text or image."));
        }

        var currentUser = (User)httpContext.Items[AuthFilter.UserItemKey]!;
        var myId = currentUser.Id;
        var receiverId = id;

        var users = mongoDb.GetCollection<User>(UsersCollection);
        var receiverExists = await users
            .Find(Builders<User>.Filter.Eq("_id", ObjectId.Parse(receiverId)))
            .Project<User>(Builders<User>.Projection.Include("_id"))
            .AnyAsync();

        if (!receiverExists)
        {
            return Results.NotFound(new ErrorResponse("Receiver not found."));
        }

        string? imageUrl = null;
        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            try
            {
                imageUrl = await cloudinary.UploadImageAsync(request.Image);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Image upload failed");
                return Results.Json(new ErrorResponse("Image upload failed."), statusCode: 500);
            }
        }

        var message = new Message
        {
            SenderId = myId,
            ReceiverId = receiverId,
            Text = request.Text,
            Image = imageUrl
        };

        var messages = mongoDb.GetCollection<Message>(MessagesCollection);
        await messages.InsertOneAsync(message);

        var responsePayload = message.ToResponse();

        var receiverConnectionId = connectionManager.GetConnectionId(receiverId);
        if (!string.IsNullOrEmpty(receiverConnectionId))
        {
            await hubContext.Clients
                .Client(receiverConnectionId)
                .SendAsync("newMessage", responsePayload);
        }

        return Results.Created($"/api/messages/{message.Id}", new SendMessageResponse(responsePayload));
    }
}

public sealed class MessageEndpointsLogger { }
