using Echoes.Server.DTOs;
using Echoes.Server.Models;

namespace Echoes.Server.Mappers;

public static class MessageMapper
{
    public static MessageResponse ToResponse(this Message message) => new(
        message.Id,
        message.SenderId,
        message.ReceiverId,
        message.Text,
        message.Image,
        message.CreatedAt,
        message.UpdatedAt);
}
