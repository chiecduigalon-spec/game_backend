using MongoDB.Bson;
using MongoDB.Driver;

namespace GameBackend.services
{
    public class autoSeeder
    {
        private readonly IMongoDatabase _database;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<autoSeeder> _logger;

        public autoSeeder(IMongoClient mongoClient, IConfiguration config, IWebHostEnvironment env, ILogger<autoSeeder> logger)
        {
            var dbName = config["DatabaseSettings:DatabaseName"] ?? "GameDataManager";
            _database = mongoClient.GetDatabase(dbName);
            _env = env;
            _logger = logger;
        }

        public async Task SyncAllJsonFilesAsync()
        {
            string dataFolder = Path.Combine(_env.ContentRootPath, "Data");
            if (!Directory.Exists(dataFolder)) return;

            // Lấy tất cả các file .json trong thư mục Data
            var jsonFiles = Directory.GetFiles(dataFolder, "*.json");

            foreach (var filePath in jsonFiles)
            {
                // Tên file (ví dụ: "questions.json") sẽ được dùng làm tên Collection ("questions")
                string collectionName = Path.GetFileNameWithoutExtension(filePath).ToLower();
                var collection = _database.GetCollection<BsonDocument>(collectionName);

                try
                {
                    string jsonContent = await File.ReadAllTextAsync(filePath);
                    var documents = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<List<BsonDocument>>(jsonContent);

                    if (documents == null || documents.Count == 0) continue;

                    int count = 0;
                    foreach (var doc in documents)
                    {
                        // Tìm khóa chính để sync: ưu tiên question_id, boss_id, id, hoặc _id
                        string idField = GetIdFieldName(doc);

                        if (!string.IsNullOrEmpty(idField) && doc.Contains(idField))
                        {
                            var filter = Builders<BsonDocument>.Filter.Eq(idField, doc[idField]);
                            await collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true });
                        }
                        else
                        {
                            // Nếu không có ID tùy chỉnh, tự động thêm/ghi đè theo _id mặc định
                            if (doc.Contains("_id"))
                            {
                                var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                                await collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true });
                            }
                            else
                            {
                                await collection.InsertOneAsync(doc);
                            }
                        }
                        count++;
                    }

                    _logger.LogInformation($"Đồng bộ thành công {count} bản ghi từ '{Path.GetFileName(filePath)}' vào collection '{collectionName}'!");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi khi đồng bộ file {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }
        }

        private string GetIdFieldName(BsonDocument doc)
        {
            foreach (var key in doc.Names)
            {
                if (key.EndsWith("_id") || key.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    return key; // Tự động nhận diện question_id, boss_id, minigame_id...
                }
            }
            return string.Empty;
        }
    }
}