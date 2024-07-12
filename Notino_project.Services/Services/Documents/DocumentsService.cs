using Microsoft.Extensions.Caching.Memory;
using Notino_project.Models.Models;
using Notino_project.Repositories.Interfaces.Repositories;
using Notino_project.Services.Interfaces.Services.Documents;

namespace Notino_project.Services.Services.Documents
{
    public class DocumentsService : IDocumentsService
    {
        private readonly IDocumentsRepository _repository;
        private readonly IMemoryCache _cache;

        private readonly string _cachePrefix = "documents";

        public DocumentsService(
            IDocumentsRepository repository,
            IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<Document> GetById(Guid id)
        {
            var cacheKey = GetCacheKey(id.ToString());

            if (!_cache.TryGetValue(cacheKey, out Document document))
            {
                document = await _repository.GetById(id);

                if (document != null)
                {
                    SetCache(cacheKey, document);
                }
            }

            return document;
        }

        public async Task<Document> Create(Document document)
        {
            var created = await _repository.Create(document);

            var cacheKey = GetCacheKey(created.Id.ToString());
            SetCache(cacheKey, created);

            return created;
        }

        public async Task<Document> Update(Document document)
        {
            var updated = await _repository.Update(document);

            var cacheKey = GetCacheKey(updated.Id.ToString());
            SetCache(cacheKey, updated);

            return updated;
        }

        private string GetCacheKey(string id) =>
            $"{_cachePrefix}_{id}";

        private void SetCache(string cacheKey, Document document)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _cache.Set(cacheKey, document, cacheEntryOptions);
        }
    }
}
