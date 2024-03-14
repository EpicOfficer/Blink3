using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[Group("todo", "Simple todo list")]
public class TodoModule(IUserTodoRepository todoRepository) : BlinkModuleBase<IInteractionContext>
{
    [SlashCommand("add", "Add an item to your todo list")]
    public async Task Add([MaxLength(25)] string label, [MaxLength(50)] string? description = null)
    {
        int count = await todoRepository.GetCountByUserIdAsync(Context.User.Id);
        if (count >= 25)
        {
            await RespondErrorAsync("Too many todo items!", "You cannot create more than 25 todo list items, please remove some first!");
            return;
        }
        
        await todoRepository.AddAsync(new UserTodo
        {
            UserId = Context.User.Id,
            Label = label,
            Description = description,
            Complete = false
        });

        await RespondSuccessAsync($"Created todo item \"{label}\"");
    }
    
    [SlashCommand("view", "View your todo list")]
    public async Task View()
    {
        IReadOnlyCollection<UserTodo> todos = await todoRepository.GetByUserIdAsync(Context.User.Id);
        if (todos.Count < 1)
        {
            await RespondInfoAsync("You don't have any todo items!");
            return;
        }

        EmbedFieldBuilder[] fields = todos.Select(todo => new EmbedFieldBuilder()
        {
            Name = $"{(todo.Complete ? Icons.BoxChecked : Icons.Box)} {todo.Label}",
            Value = todo.Description ?? "_ _",
            IsInline = false
        }).ToArray();
        
        IGuildUser? user = Context.User as IGuildUser;
        await RespondPlainAsync($"{(user is null ? "Your" : user.GetFriendlyName() + "'s")} todo list", embedFields: fields);
    }
}