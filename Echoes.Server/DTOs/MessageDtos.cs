namespace Echoes.Server.DTOs;

public record SendMessageRequest(string? Text, string? Image);

public record MessageResponse(
    string Id,
    string SenderId,
    string ReceiverId,
    string? Text,
    string? Image,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record UsersListResponse(IReadOnlyList<UserResponse> Users);

public record MessagesListResponse(IReadOnlyList<MessageResponse> Messages);

public record SendMessageResponse(MessageResponse Message);
