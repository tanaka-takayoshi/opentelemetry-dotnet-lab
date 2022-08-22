using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) => 
{
    logger.LogInformation("テスト");
    return "Hello World!";
});

app.MapGet("/External", async (ILogger<Program> logger) => 
{
    var client = new HttpClient();
    try
    {
        await client.GetStringAsync("http://example.com");
        logger.LogInformation("テスト");
        return "Hello World!";
    }
    catch (System.Exception)
    {
        throw;
    }
    
});

app.MapGet("/ExternalError", async (ILogger<Program> logger) => 
{
    var activity = Activity.Current;
    var client = new HttpClient();
    try
    {
        await client.GetStringAsync("https://httpstat.us/502");
        return "Hello World!";
    }
    catch (System.Exception e)
    {
        activity?.SetStatus(ActivityStatusCode.Error, "外部呼び出しエラー");
        logger.LogError(e, "外部呼び出しエラー");
        return "error";
    }
    
});

app.Run();
