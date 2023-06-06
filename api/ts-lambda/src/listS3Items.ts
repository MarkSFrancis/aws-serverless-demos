import { S3Client, ListObjectsV2Command } from '@aws-sdk/client-s3';
import { APIGatewayProxyHandlerV2 } from 'aws-lambda';

const bucketName = process.env.BUCKET_NAME;
const client = new S3Client({});

export const handler: APIGatewayProxyHandlerV2 = async (_event, _context) => {
  const response = await client.send(new ListObjectsV2Command({
    Bucket: bucketName
  }));

  const items = response.Contents?.map(c => c.Key).filter((k): k is string => !!k) ?? [];
  console.log(`Scanned ${bucketName}, found ${items.length} items`)

  return {
    body: JSON.stringify({ items }),
    statusCode: 200,
  }
}
