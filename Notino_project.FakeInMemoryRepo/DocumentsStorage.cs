using Notino_project.FakeInMemoryRepo.StorageModels;
using System.Collections.Concurrent;

namespace Notino_project.FakeInMemoryRepo
{
    public static class DocumentsStorage
    {
        public static ConcurrentDictionary<string, DocumentDal> Documents = new();
    }
}
