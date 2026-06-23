using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Echoes.Server.Models;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SenderId { get; set; } = string.Empty;

    [BsonElement("receiverId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ReceiverId { get; set; } = string.Empty;

    [BsonElement("text")]
    public string? Text { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
