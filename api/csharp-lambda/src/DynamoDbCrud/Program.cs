using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IAmazonDynamoDB, AmazonDynamoDBClient>();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/dynamodb/{*key}", async ([FromServices] IAmazonDynamoDB dynamoClient, [FromServices] IConfiguration config, [FromServices] ILogger<Program> logger, string key) =>
{
    var dynamoContext = new DynamoDBContext(dynamoClient);

    var queryExecutor = dynamoContext.QueryAsync<DynamoItem>(key);

    var items = new List<DynamoItem>();
    while (!queryExecutor.IsDone)
    {
        items.AddRange(await queryExecutor.GetNextSetAsync());
    }

    logger.LogInformation($"Queried {key}, found {items.Count} items");

    return Results.Ok(new
    {
        Items = items,
    });
});

app.MapPost("/dynamodb/{*key}", async ([FromServices] IAmazonDynamoDB dynamoClient, [FromServices] IConfiguration config, [FromServices] ILogger<Program> logger, string id, [FromBody] DynamoItemDto req) =>
{
    logger.LogInformation($"Adding item to {id}");

    var item = new DynamoItem
    {
        Id = id,
        CreatedOn = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
        Content = req.content
    };

    var dynamoContext = new DynamoDBContext(dynamoClient);
    await dynamoContext.SaveAsync(item);

    return Results.Created($"/dynamodb/{item.Id}", item);
});

app.Run();

record DynamoItemDto(string content);

[DynamoDBTable("CsMoonstarItems")]
class DynamoItem
{
    public string Id { get; set; } = "";
    public string CreatedOn { get; set; } = "";
    public string Content { get; set; } = "";
}
