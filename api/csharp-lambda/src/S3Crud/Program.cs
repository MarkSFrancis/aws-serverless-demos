using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IAmazonS3, AmazonS3Client>();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/s3", async ([FromServices] IAmazonS3 s3Client, [FromServices] IConfiguration config, [FromServices] ILogger<Program> logger) =>
{
    var bucketName = config["BUCKET_NAME"];
    var s3Items = await s3Client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
    {
        BucketName = bucketName,
    });

    var itemKeys = s3Items.S3Objects.Select(o => o.Key).ToList();
    logger.LogInformation($"Scanned {bucketName}, found {itemKeys} items");

    return Results.Ok(new
    {
        Items = itemKeys,
    });
});

app.MapGet("/s3/{*id}", async ([FromServices] IAmazonS3 s3Client, [FromServices] IConfiguration config, [FromServices] ILogger<Program> logger, string id) =>
{
    var bucketName = config["BUCKET_NAME"];
    logger.LogInformation($"Fetching item {id} from bucket {bucketName}");

    var s3Item = await s3Client.GetObjectAsync(bucketName, id);

    Results.Stream(s3Item.ResponseStream, "text/plain");
});

app.Run();
