using System.Text.Json;
using GreenDonut.Data.Cursors;
using Host;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();

var services = builder.Services;

services.AddDbContext<PlaygroundDbContext>((sp, options) => { options.UseInMemoryDatabase("Playground"); });

CursorKeySerializerRegistration.Register(new SomeEnumCursorKeySerializer());

var requestExecutor = await services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddSorting()
    .AddProjections()
    .AddFiltering()
    .AddDbContextCursorPagingProvider()
    // .AddCursorPagingProvider<CustomPagingProvider>()
    .InitializeOnStartup()
    .BuildRequestExecutorAsync();


var app = builder.Build();
app.MapGraphQL();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PlaygroundDbContext>();
SeedDb(dbContext);

var initialResponse = await requestExecutor.ExecuteAsync("""
                                                         query allUsers {
                                                           allUsers(
                                                             order: [{ someEnum: ASC, id: ASC }]) {
                                                             edges {
                                                               cursor
                                                               node {
                                                                 id
                                                                 someEnum
                                                               }
                                                             }
                                                           }
                                                         }
                                                         """);

var initialJson = initialResponse.ToJson();

Console.WriteLine(initialJson);

var cursors = ExtractCursors(initialJson).ToList();

var secondaryResponse = await requestExecutor.ExecuteAsync("""
                                                           query allUsers($after: String!) {
                                                             allUsers(
                                                               order: [{ someEnum: ASC, id: ASC }]
                                                               after: $after) {
                                                               edges {
                                                                 cursor
                                                                 node {
                                                                   id
                                                                   someEnum
                                                                 }
                                                               }
                                                             }
                                                           }
                                                           """, new Dictionary<string, object?>
{
    { "after", cursors.First() }
});
var secondaryJson = secondaryResponse.ToJson();
Console.WriteLine(secondaryJson);

await app.RunAsync();

IEnumerable<string> ExtractCursors(string s)
{
    using JsonDocument doc = JsonDocument.Parse(s);
    var edges = doc.RootElement
        .GetProperty("data")
        .GetProperty("allUsers")
        .GetProperty("edges");

    foreach (JsonElement edge in edges.EnumerateArray())
    {
        string cursor = edge.GetProperty("cursor").GetString();
        yield return cursor;
    }
}

void SeedDb(PlaygroundDbContext playgroundDbContext)
{
    playgroundDbContext.Users.AddRange([
        new User()
        {
            Id = Guid.NewGuid(),
            SomeEnum = SomeEnum.None
        },
        new User()
        {
            Id = Guid.NewGuid(),
            SomeEnum = SomeEnum.Foo
        },
        new User()
        {
            Id = Guid.NewGuid(),
            SomeEnum = SomeEnum.Bar
        }
    ]);
    playgroundDbContext.SaveChanges();
}