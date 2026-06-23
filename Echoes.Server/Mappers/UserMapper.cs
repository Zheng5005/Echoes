using Echoes.Server.DTOs;
using Echoes.Server.Models;

namespace Echoes.Server.Mappers;

public static class UserMapper
{
    public static UserResponse ToResponse(this User user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.ProfilePic,
        user.CreatedAt,
        user.UpdatedAt);
}
