using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using System.Net;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class HttpApiJsonSerializerContext : JsonSerializerContext
{
}

record Response(List<string> ItemKeys);

public class Function
{
    IAmazonS3 S3Client { get; }
    string BucketName { get; }

    public Function() : this(new AmazonS3Client(), System.Environment.GetEnvironmentVariable("BUCKET_NAME")!)
    { }

    /// <summary>
    /// Used for tests
    /// </summary>
    public Function(IAmazonS3 s3Client, string bucketName)
    {
        this.S3Client = s3Client;
        BucketName = bucketName;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest evnt, ILambdaContext context)
    {
        var s3Items = await S3Client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
        {
            BucketName = BucketName,
        });

        var itemKeys = s3Items.S3Objects.Select(o => o.Key).ToList();
        context.Logger.LogInformation($"Scanned {BucketName}, found {itemKeys.Count} items");

        var response = new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(new Response(itemKeys)),
            Headers = new Dictionary<string, string>
            {
                {
                    "Content-Type",
                    "application/json"
                }
            }
        };

        return response;
    }
}
