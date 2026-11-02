using MongoDB.Driver;
using MongoDB.Bson;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Đăng ký MongoDB Client (Singleton)
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

// Đăng ký Mongo Database 
var databaseName = builder.Configuration["DatabaseSettings:DatabaseName"];
builder.Services.AddScoped(sp =>
{
    return sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName);
});

// Đăng ký Redis ConnectionMultiplexer (Singleton)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

var app = builder.Build();

try
{
    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var redisDb = redis.GetDatabase();
    redisDb.Ping();
    Console.WriteLine("Kết nối thành công tới Redis");

} catch (Exception ex)
{
   Console.WriteLine($"Lỗi kết nối tới Redis: {ex.Message}"); 
}

try
{
    var mongoClient = app.Services.GetRequiredService<IMongoClient>();
    mongoClient.GetDatabase("GameDataSystem").RunCommand<BsonDocument>(new BsonDocument { { "ping", 1 } });
    Console.WriteLine("Kết nối thành công tới MongoDB");

} catch (Exception ex)
{
    Console.WriteLine($"Lỗi kết nối tới MongoDB: {ex.Message}");
}

app.UseAuthorization();
app.MapControllers();

// // Add services to the container.
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
