using System.Net.Http.Json;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Models;
using Blink3.Web.Interfaces;

namespace Blink3.Web.Repositories;

/// <inheritdoc />
public class TodoHttpRepository(HttpClient httpClient) : ITodoHttpRepository
{
    private const string BasePath = "api/todo";

    public async Task<IReadOnlyCollection<UserTodo>> GetAsync() =>
        await httpClient.GetFromJsonAsync<IReadOnlyCollection<UserTodo>>($"{BasePath}") ?? [];

    public Task<UserTodo> GetAsync(int id)
    {
        throw new NotImplementedException(); 
    }

    public Task<UserTodo> AddAsync(UserTodoDto todoDto)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(int id, UserTodoDto todoDto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}