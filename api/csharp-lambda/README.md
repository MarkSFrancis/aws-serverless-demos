# C# Lambdas with CDK

This is a project that demos how to build C# lambdas (using both raw functions and C# minimal APIs) using the C# CDK.

This app is **not** designed for use in production. Instead, we recommend you use it as an example for building serverless C# apps on AWS

## Prerequisites

- [An AWS account](https://aws.amazon.com/getting-started/)
- [AWS CDK](https://docs.aws.amazon.com/cdk/v2/guide/home.html)
- [Docker CLI](https://www.docker.com/) (Docker Desktop is not required)
- [.NET 6 SDK](https://dotnet.microsoft.com/)

## Demo contents

### Deployed resources

- An S3 bucket with the bucket name `"moonstar-sample-bucket"`
- A lambda which returns the object keys in the above S3 bucket
  - This lambda is a pure functional lambda, using source generators for JSON serialization
  - Unit tests are included for this lambda
- A lambda which returns "Hello world" when executed
  - This lambda uses C# minimal APIs, with a single endpoint mapped to `GET /`
  - Unit tests are not included for this lambda
- A HTTP API
  - Which executes the list-s3 lambda on `GET /`
  - Which executes the S3Crud lambda on `GET /s3` or `GET /s3/{anything+}`

### Project structure

- `src`
  - `GetFromS3` - A functional lambda which lists the contents of a generated S3 bucket
    - `src`
      - `GetFromS3`
        - `Function.cs` - The lambda source code
        - `aws-lambda-tools-defaults.json` - Configuration for the lambda runtime
        - `GetFromS3.csproj` - Lists the lambda's dependencies
    - `test`
      - `GetFromS3.Tests`
        - `FunctionTest.cs` - Unit tests for the lambda
        - `GetFromS3.Test.csproj`
  - `S3Crud` - A minimal API with a range of endpoints for S3
    - `Program.cs` - The minimal API source
    - `appsettings.json` - Configuration for the API
    - `aws-lambda-tools-defaults.json` - Configuration for the lambda runtime
    - `S3Crud.csproj`
  - `Infrastructure`
    - `Infrastructure.cs` - Contains the IaC for the CDK stack
- `cdk.json` - Contains CDK configuration values

## Useful commands

- `cdk synth`        emits the synthesized CloudFormation template
- `cdk deploy`       synth + deploy this stack to your default AWS account/region

## Performance notes

- Warm lambdas take around 50-100ms to process a request

|Lambda instance|Function or minimal API|Lambda size|Cold start time|
|--|--|--|--|
|Hello world|Function|256MB|1s|
||Minimal API|256MB|1s|
|S3 read|Function|256MB|1.5s|
||Minimal API|256MB|4s|
|DynamoDB read|Minimal API|128MB|8.4s|
|||256MB|4.3s|
|||512MB|2.3s|
