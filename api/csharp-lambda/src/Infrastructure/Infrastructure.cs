using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2.Alpha;
using Amazon.CDK.AWS.Apigatewayv2.Integrations.Alpha;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;


namespace Infrastructure
{
    public class CsharpLambdaStack : Stack
    {
        const int LAMBDA_MEMORY_SIZE = 128;

        internal CsharpLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var bucket = new Bucket(this, "cs-s3-bucket", new BucketProps
            {
                BucketName = "cs-moonstar-sample-bucket",
                RemovalPolicy = RemovalPolicy.DESTROY,
                AutoDeleteObjects = true,
            });

            var dynamoTable = new Table(this, "cs-sample-table", new TableProps()
            {
                TableName = "CsMoonstarItems",
                PartitionKey = new Attribute
                {
                    Name = "Id",
                    Type = AttributeType.STRING
                },
                SortKey = new Attribute
                {
                    Name = "CreatedOn",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            var buildOptions = new BundlingOptions
            {
                Image = Runtime.DOTNET_6.BundlingImage,
                User = "root",
                OutputType = BundlingOutput.ARCHIVED,
                Command = new string[]{
                    "/bin/sh",
                    "-c",
                    " dotnet tool install -g Amazon.Lambda.Tools" +
                    " && dotnet build" +
                    " && dotnet lambda package --output-package /asset-output/function.zip"
                }
            };

            var listS3ItemsFn = new Function(this, "cs-list-s3-items", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = LAMBDA_MEMORY_SIZE,
                Handler = "GetFromS3::Function::FunctionHandler",
                Code = Code.FromAsset("src/GetFromS3/src/GetFromS3", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOptions
                }),
                Environment = new Dictionary<string, string>
                {
                    {
                        "BUCKET_NAME",
                        bucket.BucketName
                    }
                },
                Timeout = Duration.Seconds(10),
                Tracing = Tracing.ACTIVE
            });
            bucket.GrantRead(listS3ItemsFn);
            var listS3ItemsIntegration = new HttpLambdaIntegration("CsListS3Integration", listS3ItemsFn);

            var s3CrudFn = new Function(this, "cs-s3-crud", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = LAMBDA_MEMORY_SIZE,
                Handler = "S3Crud",
                Code = Code.FromAsset("src/S3Crud", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOptions
                }),
                Environment = new Dictionary<string, string>
                {
                    {
                        "BUCKET_NAME",
                        bucket.BucketName
                    }
                },
                Tracing = Tracing.ACTIVE,
                Timeout = Duration.Seconds(10),
            });
            bucket.GrantRead(s3CrudFn);
            var s3CrudIntegration = new HttpLambdaIntegration("CsS3CrudIntegration", s3CrudFn);

            var dynamoDbFn = new Function(this, "cs-dynamodb-crud", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = LAMBDA_MEMORY_SIZE,
                Handler = "DynamoDbCrud",
                Code = Code.FromAsset("src/DynamoDbCrud", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOptions
                }),
                Tracing = Tracing.ACTIVE,
                Timeout = Duration.Seconds(10),
            });
            dynamoTable.GrantReadWriteData(dynamoDbFn);
            var dynamoDbIntegration = new HttpLambdaIntegration("CsS3CrudIntegration", dynamoDbFn);

            var httpApi = new HttpApi(this, "hello-api");

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/",
                Methods = new[] { Amazon.CDK.AWS.Apigatewayv2.Alpha.HttpMethod.GET },
                Integration = listS3ItemsIntegration,
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/s3",
                Methods = new[] { Amazon.CDK.AWS.Apigatewayv2.Alpha.HttpMethod.GET },
                Integration = s3CrudIntegration,
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/s3/{id+}",
                Methods = new[] { Amazon.CDK.AWS.Apigatewayv2.Alpha.HttpMethod.GET },
                Integration = s3CrudIntegration,
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/dynamodb/{key+}",
                Methods = new[]
                {
                    Amazon.CDK.AWS.Apigatewayv2.Alpha.HttpMethod.GET,
                    Amazon.CDK.AWS.Apigatewayv2.Alpha.HttpMethod.POST
                },
                Integration = dynamoDbIntegration,
            });

            new CfnOutput(this, "CsApiUrl", new CfnOutputProps
            {
                ExportName = "CsApiUrl",
                Value = httpApi.ApiEndpoint,
            });
        }
    }
}
