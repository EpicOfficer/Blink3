using System.Net.Http.Json;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Models;
using Blink3.Web.Interfaces;

namespace Blink3.Web.Repositories;

/// <inheritdoc />
public class TodoHttpRepository(HttpClient httpClient) : ITodoHttpRepository
{
    private const string BasePath = "api/todo";

    public async Task<IEnumerable<UserTodo>> GetAsync()
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<UserTodo>>($"{BasePath}") ?? [];
    }

    public async Task<UserTodo?> GetAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<UserTodo>($"{BasePath}/{id}");
    }

    public async Task<UserTodo> AddAsync(UserTodoDto todoDto)
    {
        HttpResponseMessage resp = await httpClient.PostAsJsonAsync($"{BasePath}", todoDto);
        if (!resp.IsSuccessStatusCode)
            throw new ApplicationException("Error occured while adding todo item",
                new Exception(await resp.Content.ReadAsStringAsync()));

        return await resp.Content.ReadFromJsonAsync<UserTodo>() ??
               throw new InvalidOperationException("Unable to deserialize JSON response from API");
    }

    public async Task UpdateAsync(int id, UserTodoDto todoDto)
    {
        await httpClient.PutAsJsonAsync($"{BasePath}/{id}", todoDto);
    }

    public async Task DeleteAsync(int id)
    {
        await httpClient.DeleteAsync($"{BasePath}/{id}");
    }
}