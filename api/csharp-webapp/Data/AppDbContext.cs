using Microsoft.EntityFrameworkCore;

namespace NewsfeedApiCSharpWebApp.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; } = null!;
}
