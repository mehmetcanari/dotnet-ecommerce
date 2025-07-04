using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Domain.Model;

public class Counter
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("sequence_value")]
    public int SequenceValue { get; set; }
} 