import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { APIGatewayProxyHandlerV2 } from 'aws-lambda';
import { DynamoDBDocumentClient, QueryCommand } from "@aws-sdk/lib-dynamodb";

const tableName = process.env.TABLE_NAME;

interface DynamoItem {
  id: string;
  createdOn: string;
  content: string;
}

const client = new DynamoDBClient({
  region: "eu-west-1"
});

const docClient = DynamoDBDocumentClient.from(client);

export const handler: APIGatewayProxyHandlerV2 = async (event, _context) => {
  const { key } = event.pathParameters as { key: string };

  const response = await docClient.send(new QueryCommand({
    TableName: tableName,
    KeyConditionExpression: 'id = :id',
    ExpressionAttributeValues: {
      ':id': key,
    }
  }));

  const items = response.Items as DynamoItem[] ?? [];
  console.log(`Scanned ${tableName}, found ${items.length} items`)

  return {
    body: JSON.stringify({ items }),
    statusCode: 200,
  }
}
