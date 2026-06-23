namespace Echoes.Server.DTOs;

public record SignupRequest(string Email, string FullName, string Password);

public record LoginRequest(string Email, string Password);

public record UpdateProfileRequest(string ProfilePic);

public record UserResponse(
    string Id,
    string Email,
    string FullName,
    string ProfilePic,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record AuthResponse(string Message, UserResponse? User);

public record CheckAuthResponse(UserResponse? User);

public record ErrorResponse(string Message);
