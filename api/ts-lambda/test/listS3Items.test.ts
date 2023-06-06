import { APIGatewayProxyEventV2, Context } from 'aws-lambda';
import { handler } from '../src/listS3Items';
import { ListObjectsV2Command, ListObjectsV2CommandOutput, S3Client } from '@aws-sdk/client-s3';

jest.mock('@aws-sdk/client-s3', () => {
  const sendFn = jest.fn();

  return {
    S3Client: jest.fn().mockImplementation((): S3Client => ({
      send: sendFn as S3Client["send"]
    }) as S3Client),
    ListObjectsV2Command: jest.fn(),
  }
});

const sendMock = jest.mocked<
  (command: ListObjectsV2Command) => Promise<ListObjectsV2CommandOutput>
>(new S3Client({}).send, { shallow: true });

beforeEach(() => {
  jest.resetAllMocks();
});

it('should return 200 even if no objects are in the bucket', async () => {
  sendMock.mockResolvedValue({
    $metadata: undefined as any,
    Contents: [],
  });

  const response = await handler({} as APIGatewayProxyEventV2, {} as Context, {} as any);
  
  expect(typeof response).toEqual("object");

  if (typeof response === "object") {
    expect(response.statusCode).toBe(200);
    expect(response.body).toEqual(JSON.stringify({ items: [] }))
  }
});

it('should return 200 with object keys in bucket', async () => {
  const keys = ['key1', 'key2', '_key3_'];

  sendMock.mockResolvedValue({
    $metadata: undefined as any,
    Contents: keys.map((k) => ({
      Key: k,
    })),
  });

  const response = await handler({} as APIGatewayProxyEventV2, {} as Context, {} as any);
  
  expect(typeof response).toEqual("object");

  if (typeof response === "object") {
    expect(response.statusCode).toBe(200);
    expect(response.body).toEqual(JSON.stringify({ items: keys }))
  }
})
