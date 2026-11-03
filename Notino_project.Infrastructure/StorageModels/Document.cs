using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Notino_project.Infrastructure.StorageModels
{
    public sealed class Document : StorageModel
    {
        public List<string> Tags { get; set; } = [];

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
