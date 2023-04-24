using System.ComponentModel.DataAnnotations;

namespace NewsfeedApiCSharpWebApp.Data;

public class Post
{
    [Required]
    public string Id { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }
}