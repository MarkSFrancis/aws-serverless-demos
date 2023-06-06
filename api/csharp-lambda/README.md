# Welcome to your CDK C# project!

This is a project that demos how to build C# lambdas (using both raw functions and C# minimal APIs) using the C# CDK.

## Prerequisites

- [An AWS account](https://aws.amazon.com/getting-started/)
- [AWS CDK](https://docs.aws.amazon.com/cdk/v2/guide/home.html)
- [Docker CLI](https://www.docker.com/) (Docker Desktop is not required)

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
  - Which executes the list-s3 lambda on `GET /s3`
  - Which executes the hello-world lambda on `GET /`

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
  - `HelloWorld` - A minimal API with a single "Hello World" endpoint
    - `Program.cs` - The minimal API source
    - `appsettings.json` - Configuration for the API
    - `aws-lambda-tools-defaults.json` - Configuration for the lambda runtime
    - `HelloWorld.csproj`
  - Infrastructure
    - `Infrastructure.cs` - Contains the IaC for the CDK stack
- `cdk.json` - Contains CDK configuration values

## Useful commands

- `cdk synth`        emits the synthesized CloudFormation template
- `cdk deploy`       synth + deploy this stack to your default AWS account/region
