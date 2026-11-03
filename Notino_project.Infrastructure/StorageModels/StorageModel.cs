using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Notino_project.Infrastructure.StorageModels
{
    public abstract class StorageModel
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; } = Guid.Empty;

        public bool IsDeleted { get; set; }
    }
}
