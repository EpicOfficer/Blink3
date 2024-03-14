using Blink3.Common.Caching;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IUserTodoRepository"/>
public class UserTodoRepository(BlinkDbContext dbContext) :
    GenericRepository<UserTodo>(dbContext), IUserTodoRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<UserTodo>> GetByUserIdAsync(ulong userId)
    {
        return await _dbContext.UserTodos
            .Where(u => u.UserId.Equals(userId))
            .ToListAsync();
    }
}