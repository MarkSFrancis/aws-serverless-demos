import { HttpApi, HttpMethod } from '@aws-cdk/aws-apigatewayv2-alpha';
import { HttpLambdaIntegration } from '@aws-cdk/aws-apigatewayv2-integrations-alpha';
import * as cdk from 'aws-cdk-lib';
import { Architecture, Runtime, Tracing } from 'aws-cdk-lib/aws-lambda';
import { NodejsFunction } from 'aws-cdk-lib/aws-lambda-nodejs';
import { Bucket } from 'aws-cdk-lib/aws-s3';
import { Construct } from 'constructs';

export class TsLambdaStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const bucket = new Bucket(this, "ts-ExampleBucket", {
      bucketName: "ts-moonstar-sample-bucket",
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
    });

    const listS3ItemsFn = new NodejsFunction(this, "ts-list-s3-items", {
      runtime: Runtime.NODEJS_18_X,
      memorySize: 128,
      entry: 'src/listS3Items.ts',
      handler: 'handler',
      bundling: {
        minify: true,
      },
      environment: {
        BUCKET_NAME: bucket.bucketName,
      },
      tracing: Tracing.ACTIVE,
      architecture: Architecture.ARM_64,
    });
    bucket.grantRead(listS3ItemsFn);
    const listS3ItemsIntegration = new HttpLambdaIntegration("ts-list-s3-integration", listS3ItemsFn);

    const httpApi = new HttpApi(this, "ts-hello-api");

    httpApi.addRoutes({
      path: "/",
      methods: [HttpMethod.GET],
      integration: listS3ItemsIntegration,
    });

    new cdk.CfnOutput(this, "ts-api-url", {
      exportName: "TsApiUrl",
      value: httpApi.apiEndpoint,
    });
  }
}
