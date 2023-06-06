# C# Lambdas with CDK

This is a project that demos how to build typescript lambdas using typescript CDK.

This app is **not** designed for use in production. Instead, we recommend you use it as an example for building serverless C# apps on AWS

## Prerequisites

- [An AWS account](https://aws.amazon.com/getting-started/)
- [AWS CDK](https://docs.aws.amazon.com/cdk/v2/guide/home.html)
- [NodeJS](https://nodejs.org)

## Demo contents

### Deployed resources

- An S3 bucket with the bucket name `"ts-moonstar-sample-bucket"`
- A lambda which returns the object keys in the above S3 bucket
- A HTTP API
  - Which executes the list-s3 lambda on `GET /`

### Project structure

- `src`
  - `listS3Items.ts` - A lambda which lists the contents of a generated S3 bucket
- `lib`
  - `ts-lambda-stack.ts` - Contains the IaC for the CDK stack
- `test`
  - `listS3Items.test.ts` - Tests for the lambda
- `cdk.json` - Contains CDK configuration values

## Useful commands

- `cdk synth`        emits the synthesized CloudFormation template
- `cdk deploy`       synth + deploy this stack to your default AWS account/region

## Performance notes

- Cold starts take around 1 second
- Subsequent requests take around 60-80ms
