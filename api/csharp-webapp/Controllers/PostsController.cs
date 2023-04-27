using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsfeedApiCSharpWebApp.Data;

namespace NewsfeedApiCSharpWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    public AppDb Db { get; }

    public PostsController(AppDb db)
    {
        Db = db;
    }

    [HttpGet]
    public async Task<IEnumerable<Post>> Get() => await Db.Posts.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> Get(string id)
    {
        var result = await Db.Posts.SingleOrDefaultAsync(p => p.Id == id);

        if (result is null) return NotFound();

        return result;
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Create(Post post)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await Db.Posts.AnyAsync(p => p.Id == post.Id)) return Conflict();

        Db.Posts.Add(post);
        await Db.SaveChangesAsync();
        return post;
    }
}
