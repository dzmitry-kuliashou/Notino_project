using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Notino_project.Core.Repositories;

namespace Notino_project.Infrastructure
{
    public static class MongoDbServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDb<TDocument, TReadRepository, TWriteRepository>(
            this IServiceCollection services,
            MongoDbSettings settings)
            where TDocument : class
            where TReadRepository : class, IDocumentsReadRepository
            where TWriteRepository : class, IDocumentsWriteRepository
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(settings);

            if (string.IsNullOrWhiteSpace(settings.ConnectionString) ||
                string.IsNullOrWhiteSpace(settings.DatabaseName) ||
                string.IsNullOrWhiteSpace(settings.CollectionName))
            {
                throw new InvalidOperationException(
                    "MongoDB connection string, database name, and collection name must be configured.");
            }

            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);

            services.AddSingleton<IMongoClient>(_ => new MongoClient(clientSettings));
            services.AddSingleton(provider =>
                provider.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName));
            services.AddSingleton(provider =>
                provider.GetRequiredService<IMongoDatabase>().GetCollection<TDocument>(settings.CollectionName));
            services.AddScoped<IDocumentsReadRepository, TReadRepository>();
            services.AddScoped<IDocumentsWriteRepository, TWriteRepository>();

            return services;
        }
    }
}
