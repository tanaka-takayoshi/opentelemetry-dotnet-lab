﻿using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<MyDbContext>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<MyDbContext>();
    context.Database.EnsureCreated();
    if (!context.Persons.Any())
    {
        var persons = new []
        {
            new Person(){Id=1,Age=57,Name="Shinji"},
            new Person(){Id=2,Age=25,Name="Taro"},
            new Person(){Id=3,Age=17,Name="Jiro"}
        };
        context.Persons.AddRange(persons);
        context.SaveChanges();
    }
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    logger.LogInformation("天候情報 {Length}件取得", forecast.Length);
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/external", async (ILogger<Program> logger) =>
{
    logger.LogInformation($"HTTP呼び出し");
    var client = new HttpClient();
    var res = await client.GetAsync("http://httpbin.org/get");
    var content = await res.Content.ReadAsStringAsync();
    return content;
});

app.MapGet("/external/error", async (ILogger<Program> logger) =>
{
    logger.LogInformation($"HTTP呼び出し先がエラーを返す処理");
    var client = new HttpClient();
    var res = await client.GetAsync("http://httpbin.org/status/502");
    logger.LogInformation("外部HTTPレスポンスコード {StatusCode}", res.StatusCode);
    return res.StatusCode;
});

app.MapGet("/error", (ILogger<Program> logger) =>
{
    logger.LogError("例外が発生しました");
    throw new Exception("えらー");
});

app.MapGet("/db", (MyDbContext context, ILogger<Program> logger) =>
{
    var persons = context.Persons.ToArray();
    logger.LogInformation("DB呼び出し {Length}件取得", persons.Length);
    return persons;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class MyDbContext : DbContext  
{  
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)  
    {  
        base.OnConfiguring(optionsBuilder);  

        optionsBuilder.UseSqlite(@"Data Source='data.db'");  
    }  

    public DbSet<Person> Persons { get; set; }  
}  

public class Person  
{  
    public int Id { get; set; }  
    public int Age { get; set; }  
    public string? Name { get; set; }  
} 