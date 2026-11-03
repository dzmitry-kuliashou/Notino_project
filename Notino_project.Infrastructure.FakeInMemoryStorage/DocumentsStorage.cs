using Notino_project.Infrastructure.FakeInMemoryStorage.StorageModels;
using System.Collections.Concurrent;

namespace Notino_project.Infrastructure.FakeInMemoryStorage
{
    public static class DocumentsStorage
    {
        public static ConcurrentDictionary<string, DocumentDal> Documents = new();
    }
}
