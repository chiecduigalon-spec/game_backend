using MongoDB.Driver;

namespace GameBackend.migrations
{
    public interface IMigration
    {
        string Version { get; }
        string Description { get; }
        Task UpAsync(IMongoDatabase database, IWebHostEnvironment env);
    }
}