# Typescript Lambdas with CDK

This is a project that demos how to build typescript lambdas using typescript CDK.

This app is **not** designed for use in production. Instead, we recommend you use it as an example for building serverless typescript apps on AWS

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
  - `dynamoDbGet.ts` - A lambda which gets the items from dynamodb in a given key, sorted by date created
  - `dynamoDbPost.ts` - A lambda which adds a new item to dynamodb for a given key
- `lib`
  - `ts-lambda-stack.ts` - Contains the IaC for the CDK stack
- `test`
  - `listS3Items.test.ts` - Tests for the `listS3Items.ts` lambda
- `cdk.json` - Contains CDK configuration values

## Useful commands

- `cdk synth`        emits the synthesized CloudFormation template
- `cdk deploy`       synth + deploy this stack to your default AWS account/region

## Performance notes

- Warm lambdas take around 60-80ms to process a request
- Adding a new lambda adds a few milliseconds to the `cdk synth` time as `esbuild` is quick to build and minify the typescript and does not need to run in a container

|Lambda instance|Lambda size|Cold start time|
|--|--|--|
|S3 read|128MB|2.4s|
||256MB|1.3s|
||512MB|0.9s|
|DynamoDB read|128MB|2.4s|
||256MB|1.3s|
||512MB|0.9s|
