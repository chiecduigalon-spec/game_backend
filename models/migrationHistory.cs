using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameBackend.models
{
    public class MigrationHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        [BsonElement("version")]
        public string? Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
    }
}