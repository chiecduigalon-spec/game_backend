// using System.Text.Json;
// using GameBackend.models;
// using MongoDB.Driver;

// namespace GameBackend.migrations
// {
//     public class V1_0_0__SeedQuestionsFromJson : IMigration {
//         // 1. Khai báo Version để sửa lỗi CS0535
//             public string Version => "1.0.0";

//             // 2. Khai báo Description để sửa lỗi CS0535
//             public string Description => "Import danh sách câu hỏi ban đầu từ file JSON";
//         private readonly IMongoCollection<Question> _questionsCollection;

//         public async Task UpAsync(IMongoDatabase database, IWebHostEnvironment env)
//         {
//             _questionsCollection = database.GetCollection<Question>("questions");
//             string filePath = Path.Combine(env.ContentRootPath, "migrations", "questions.json");
//         }

//         public void Up()
//         {
//             var questionsJson = File.ReadAllText("../Data/questions.json");
//             var questions = JsonSerializer.Deserialize<List<Question>>(questionsJson);

//             if (questions != null)
//             {
//                 _questionsCollection.InsertMany(questions);
//             }
//         }

//         public void Down()
//         {
//             _questionsCollection.DeleteMany(FilterDefinition<Question>.Empty);
//         }
//     }
// }