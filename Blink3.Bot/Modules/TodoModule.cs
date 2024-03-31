using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Bot.Modals;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[Group("todo", "Simple todo list")]
public class TodoModule(IUserTodoRepository todoRepository) : BlinkModuleBase<IInteractionContext>
{
    [ComponentInteraction("todo:addButton", true)]
    public async Task AddButton()
    {
        await RespondWithModalAsync<TodoModal>("todo:addModal");
    }

    [ModalInteraction("todo:addModal", true)]
    public async Task AddModal(TodoModal modal)
    {
        await Add(modal.Label, modal.Description);
    }

    [SlashCommand("add", "Add an item to your todo list")]
    [ModalInteraction("todo:add", true)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public async Task Add(
        [MaxLength(25)] [Description("A short label for your todo list item.  Max 25 characters")]
        string label,
        [MaxLength(50)] [Description("A longer description for the todo list item.  Max 50 characters")]
        string? description = null)
    {
        int count = await todoRepository.GetCountByUserIdAsync(Context.User.Id);
        if (count >= 25)
        {
            await RespondErrorAsync("Too many todo items!",
                "You cannot create more than 25 todo list items, please remove some first!");
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
    public async Task View(
        [Description("Whether to post the todo list so that it is visible to other users.")]
        bool postPublicly = false)
    {
        ComponentBuilder builder = new ComponentBuilder()
            .WithButton("Add", "todo:addButton")
            .WithButton("Complete", "todo:completeButton", ButtonStyle.Secondary)
            .WithButton("Remove", "todo:removeButton", ButtonStyle.Danger);

        IReadOnlyCollection<UserTodo> todos = await todoRepository.GetByUserIdAsync(Context.User.Id);
        if (todos.Count < 1)
        {
            await RespondInfoAsync("You don't have any todo items!", components: builder.Build());
            return;
        }

        EmbedFieldBuilder[] fields = todos.Select(todo => new EmbedFieldBuilder
        {
            Name = $"{(todo.Complete ? Icons.BoxChecked : Icons.Box)} {todo.Label}",
            Value = string.IsNullOrWhiteSpace(todo.Description) ? "_ _" : todo.Description,
            IsInline = false
        }).ToArray();

        IGuildUser? user = Context.User as IGuildUser;
        await RespondPlainAsync($"{(user is null ? "Your" : user.GetFriendlyName() + "'s")} todo list",
            ephemeral: !postPublicly,
            components: builder.Build(),
            embedFields: fields);
    }

    [SlashCommand("complete", "Mark a todo list item as complete")]
    [ComponentInteraction("todo:completeButton", true)]
    public async Task CompleteMenu()
    {
        MessageComponent? components = await BuildSelectMenuWithTodos("todo:complete");

        if (components is null)
        {
            await RespondInfoAsync("You don't have any todo items!");
            return;
        }

        await RespondInfoAsync("Select a todo item to mark as complete:", components: components);
    }

    [SlashCommand("remove", "Permanently remove todo list item")]
    [ComponentInteraction("todo:removeButton", true)]
    public async Task RemoveMenu()
    {
        MessageComponent? components = await BuildSelectMenuWithTodos("todo:remove");

        if (components is null)
        {
            await RespondInfoAsync("You don't have any todo items!");
            return;
        }

        await RespondInfoAsync("Select a todo item to remove:", components: components);
    }

    [ComponentInteraction("todo:complete", true)]
    public async Task Complete(string id)
    {
        int key = GetId(id);

        await todoRepository.CompleteByIdAsync(key);

        await RespondSuccessAsync("Marked todo as complete");
    }

    [ComponentInteraction("todo:remove", true)]
    public async Task Remove(string id)
    {
        int key = GetId(id);

        await todoRepository.DeleteByIdAsync(key);

        await RespondSuccessAsync("Todo item removed");
    }

    private static int GetId(string id)
    {
        if (!int.TryParse(id, out int key)) throw new ArgumentOutOfRangeException(nameof(id));

        return key;
    }

    private async Task<MessageComponent?> BuildSelectMenuWithTodos(string customId)
    {
        IReadOnlyCollection<UserTodo> todos = await todoRepository.GetByUserIdAsync(Context.User.Id);
        if (todos.Count < 1) return null;

        SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
            .WithCustomId(customId)
            .WithPlaceholder("Select todo list item")
            .WithMaxValues(1)
            .WithMaxValues(1);

        foreach (UserTodo todo in todos)
        {
            if (!string.IsNullOrWhiteSpace(todo.Description))
            {
                menuBuilder.AddOption(todo.Label, todo.Id.ToString(), todo.Description);
                continue;
            }

            menuBuilder.AddOption(todo.Label, todo.Id.ToString());
        }

        return new ComponentBuilder().WithSelectMenu(menuBuilder).Build();
    }
}