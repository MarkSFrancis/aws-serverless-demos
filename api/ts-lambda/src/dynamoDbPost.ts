import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { APIGatewayProxyHandlerV2 } from 'aws-lambda';
import { DynamoDBDocumentClient, PutCommand } from "@aws-sdk/lib-dynamodb";

const tableName = process.env.TABLE_NAME;
const client = new DynamoDBClient({
  region: "eu-west-1"
});
const docClient = DynamoDBDocumentClient.from(client);

interface DynamoItem {
  id: string;
  createdOn: string;
  content: string;
}

export const handler: APIGatewayProxyHandlerV2 = async (event, _context) => {
  const { key } = event.pathParameters as { key: string };

  const item: DynamoItem = {
    id: key,
    createdOn: new Date().toISOString(),
    content: JSON.parse(event.body ?? '')?.content ?? '',
  }

  await docClient.send(new PutCommand({
    TableName: tableName,
    Item: item,
  }));

  return {
    statusCode: 201,
  }
}
