using System.Text;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[Group("todo", "Simple todo list")]
public class TodoModule(IUserTodoRepository todoRepository) : BlinkModuleBase<SocketInteractionContext>
{
    [SlashCommand("view", "View your todo list")]
    public async Task View()
    {
        IReadOnlyCollection<UserTodo> todos = await todoRepository.GetByUserIdAsync(Context.User.Id!);
        if (todos.Count < 1)
        {
            await RespondSuccessAsync("You don't have any todo items!");
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("**Your todo list**");
        
        foreach (UserTodo todo in todos)
        {
            if (todo.Complete) sb.Append("~~");
            
            sb.AppendLine($"**{todo.Label}**");
            if (!string.IsNullOrWhiteSpace(todo.Description))
                sb.AppendLine(todo.Description);

            if (todo.Complete) sb.Append("~~");
            
            sb.AppendLine();
        }

        await RespondSuccessAsync(sb.ToString());
    }
}