using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Host;

public class Query
{
    
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> AllUsers(PlaygroundDbContext dbContext)
    {
        return dbContext.Users;
    }
}