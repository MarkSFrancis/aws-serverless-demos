using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NewsfeedApiCSharpFunctions.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        services.AddDbContext<AppDb>(opt => opt.UseInMemoryDatabase("NewsfeedApp"));
    })
    .Build();

host.Run();
