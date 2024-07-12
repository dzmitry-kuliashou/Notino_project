using System.Text.Json;

namespace Notino_project.FakeInMemoryRepo.StorageModels
{
    public class DocumentDal
    {
        public Guid Id { get; set; } = Guid.Empty;
        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
        public JsonElement Data { get; set; }
    }
}
