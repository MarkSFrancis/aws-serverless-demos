# Newsfeed CSharp Functions

This folder contains an Azure Functions solution to showing a newsfeed. 

It exposes 3 endpoints:
1. `GET /api/posts` - returns a list of posts
1. `GET /api/posts/{id}` - returns a single post
1. `POST /api/posts` - creates a new post

# Getting Started

This API can run on your local machine. To do so, you'll need to install the following:

- [C# SDK v4](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local#install-the-azure-functions-core-tools)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

# Running on your local machine

To run the app on your local machine, open a terminal at the root of this project and run the following commands:

```bash
dotnet restore
func start
```

# Notes

- There is no support for hot-reload in Azure functions dev tools. Check out [this GitHub issue](https://github.com/Azure/azure-functions-core-tools/issues/1239) for more info
- This results in much more code than a simple controller app
- This approach is not separating the endpoints any more than a monolith would
- I have not tested cold starts
