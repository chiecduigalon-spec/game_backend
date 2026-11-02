using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json; // chuyển đổi chuỗi json sang object của C#

namespace GameBackend.cotroller
{
    public class Question
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] // tự động chuyển ObjectID thành string
    public string ? Id {get; set; }
    [BsonElement("question_id")]
    public string question_id {get; set; } = string.Empty;
     [BsonElement("category")]
    public int category {get; set; }
    [BsonElement("difficulty")]
    public int difficulty {get; set; }
    [BsonElement("content")]
    public string content {get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")] // route api/ từ đầu tiên của class
public class QuestionController : ControllerBase
    {
        private readonly IMongoCollection<Question> _questionCollection;
        private readonly IDatabase _redisDb;

        public QuestionController(IMongoClient mongoClient, IConnectionMultiplexer redis, IConfiguration config)
        {
            var dbName = config["DatabaseSettings:DatabaseName"] ?? "GameDataSystem"; 
            var mongoDb = mongoClient.GetDatabase(dbName);

            _questionCollection = mongoDb.GetCollection<Question>("question");
            _redisDb = redis.GetDatabase();       
            }

    [HttpGet("{category}")]
    public async Task<IActionResult> GetQuestionsByCategory(int categoryId)
        {
            string cacheKey = $"question:{categoryId}";

            // TÌm kiếm trong Redis
            var cachedData = await _redisDb.StringGetAsync(cacheKey);
            if(!cachedData.IsNullOrEmpty)
            {
                var questionFromCache = JsonSerializer.Deserialize<List<Question>>(cachedData.ToString()!);
                return Ok(new {Source = "Redis RAM", Data = questionFromCache});
            }

            // tìm kiếm trong mongo
            var filter = Builders<Question>.Filter.Eq(q => q.category, categoryId);
            var questionFromMongo = await _questionCollection.Find(filter).ToListAsync();

            if(questionFromMongo == null || questionFromMongo.Count == 0)
            {
                return NotFound(new {Message = "Không tìm thấy câu hỏi trong danh mục này"});
            }

            //lưu kết quả vào Redis với TTL = 1h
            string serializedData = JsonSerializer.Serialize(questionFromMongo);
            await _redisDb.StringSetAsync(cacheKey, serializedData, TimeSpan.FromHours(1));

            return Ok(new { Source = "MongoDB (Disk)", Data = questionFromMongo});
        }
    }
}