using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2.Alpha;
using Amazon.CDK.AWS.Apigatewayv2.Integrations.Alpha;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace Infrastructure
{
    public class CsharpLambdaStack : Stack
    {
        internal CsharpLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var bucket = new Bucket(this, "MyFirstBucket", new BucketProps
            {
                BucketName = "moonstar-sample-bucket",
                RemovalPolicy = RemovalPolicy.DESTROY,
                AutoDeleteObjects = true,
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

            var listS3ItemsFn = new Function(this, "list-s3-items", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = 256,
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
            var listS3ItemsIntegration = new HttpLambdaIntegration("ListS3Integration", listS3ItemsFn);

            var s3CrudFn = new Function(this, "s3-crud", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = 256,
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
            var s3CrudIntegration = new HttpLambdaIntegration("S3CrudIntegration", s3CrudFn);

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

            new CfnOutput(this, "ApiUrl", new CfnOutputProps
            {
                ExportName = "ApiUrl",
                Value = httpApi.ApiEndpoint,
            });
        }
    }
}
