using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IUserTodoRepository" />
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

    public async Task<int> GetCountByUserIdAsync(ulong userId)
    {
        return await _dbContext.UserTodos.CountAsync(c => c.UserId.Equals(userId));
    }

    public async Task CompleteByIdAsync(int id)
    {
        UserTodo todo = new()
        {
            Id = id,
            UserId = 0,
            Label = "",
            Description = "",
            Complete = true
        };

        _dbContext.UserTodos.Attach(todo);
        _dbContext.Entry(todo).Property(u => u.Complete).IsModified = true;

        await _dbContext.SaveChangesAsync();
    }
}