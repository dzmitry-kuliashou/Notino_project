using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Notino_project.Infrastructure.StorageModels
{
    public sealed class Document
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; } = Guid.Empty;

        public List<string> Tags { get; set; } = [];

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, string> Data { get; set; } = new();

        public bool IsDeleted { get; set; }
    }
}
