import { HttpApi, HttpMethod } from '@aws-cdk/aws-apigatewayv2-alpha';
import { HttpLambdaIntegration } from '@aws-cdk/aws-apigatewayv2-integrations-alpha';
import * as cdk from 'aws-cdk-lib';
import { AttributeType, BillingMode, Table } from 'aws-cdk-lib/aws-dynamodb';
import { Architecture, Runtime, Tracing } from 'aws-cdk-lib/aws-lambda';
import { NodejsFunction, NodejsFunctionProps } from 'aws-cdk-lib/aws-lambda-nodejs';
import { Bucket } from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';

export class TsLambdaStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const bucket = new Bucket(this, "ts-ExampleBucket", {
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
    });

    const table = new Table(this, 'ts-sample-table', {
      partitionKey: {
        name: 'id',
        type: AttributeType.STRING,
      },
      sortKey: {
        name: 'createdOn',
        type: AttributeType.STRING,
      },
      billingMode: BillingMode.PAY_PER_REQUEST,
    });

    const listS3ItemsFn = new NodejsFunction(this, "ts-list-s3-items", {
      ...defaultFuncProps,
      entry: 'src/listS3Items.ts',
      environment: {
        BUCKET_NAME: bucket.bucketName,
      },
    });
    bucket.grantRead(listS3ItemsFn);
    const listS3ItemsIntegration = new HttpLambdaIntegration("ts-list-s3-integration", listS3ItemsFn);

    const dynamoDbGetFn = new NodejsFunction(this, "ts-dynamodb-get", {
      ...defaultFuncProps,
      entry: 'src/dynamoDbGet.ts',
      environment: {
        TABLE_NAME: table.tableName,
      },
    });
    table.grantReadData(dynamoDbGetFn);
    const dynamoDbGetIntegration = new HttpLambdaIntegration("ts-dynamodb-get-integration", dynamoDbGetFn);

    const dynamoDbPostFn = new NodejsFunction(this, "ts-dynamodb-post", {
      ...defaultFuncProps,
      entry: 'src/dynamoDbPost.ts',
      environment: {
        TABLE_NAME: table.tableName,
      },
    });
    table.grantWriteData(dynamoDbPostFn);
    const dynamoDbPostIntegration = new HttpLambdaIntegration("ts-dynamodb-post-integration", dynamoDbPostFn);

    const httpApi = new HttpApi(this, "ts-hello-api");

    httpApi.addRoutes({
      path: "/",
      methods: [HttpMethod.GET],
      integration: listS3ItemsIntegration,
    });

    httpApi.addRoutes({
      path: '/dynamodb/{key+}',
      methods: [HttpMethod.GET],
      integration: dynamoDbGetIntegration,
    });

    httpApi.addRoutes({
      path: '/dynamodb/{key+}',
      methods: [HttpMethod.POST],
      integration: dynamoDbPostIntegration,
    });

    new cdk.CfnOutput(this, "ts-api-url", {
      exportName: "TsApiUrl",
      value: httpApi.apiEndpoint,
    });
  }
}

const defaultFuncProps: NodejsFunctionProps = {
  runtime: Runtime.NODEJS_18_X,
  memorySize: 256,
  handler: 'handler',
  bundling: {
    minify: true,
  },
  tracing: Tracing.ACTIVE,
  architecture: Architecture.ARM_64,
}
