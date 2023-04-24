using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsfeedApiCSharpFunctions.Data;

namespace NewsfeedApiCSharpFunctions
{
    public class PostsFuncs
    {
        private readonly ILogger _logger;
        private readonly AppDb _db;

        public PostsFuncs(ILoggerFactory loggerFactory, AppDb db)
        {
            _logger = loggerFactory.CreateLogger<PostsFuncs>();
            _db = db;
        }

        [Function("GetPosts")]
        public async Task<HttpResponseData> GetPosts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "posts")] HttpRequestData req)
        {
            var result = await _db.Posts.ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function("GetPost")]
        public async Task<HttpResponseData> GetPost([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "posts/{id}")] HttpRequestData req, string id)
        {
            var result = await _db.Posts.SingleOrDefaultAsync(p => p.Id == id);

            if (result is null) return req.CreateResponse(HttpStatusCode.NotFound);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }

        [Function("AddPost")]
        public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "posts")] HttpRequestData req)
        {
            var post = await req.ReadFromJsonAsync<Post>();
            if (post is null) return req.CreateResponse(HttpStatusCode.BadRequest); // Does not perform any validation beyond checking that no erroneous properties are present

            if (await _db.Posts.AnyAsync(p => p.Id == post.Id)) return req.CreateResponse(HttpStatusCode.Conflict);

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var response = req.CreateResponse(HttpStatusCode.Created); // Missing Location header - not compliant with REST spec
            await response.WriteAsJsonAsync(post);
            return response;
        }
    }
}
