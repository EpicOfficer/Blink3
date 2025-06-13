using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Bot.Modals;
using Blink3.Core.Entities;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Repositories.Interfaces;
using Discord;
using Discord.Interactions;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[Group("todo", "Simple todo list")]
public class TodoModule(IUnitOfWork unitOfWork) : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
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
        int count = await _unitOfWork.UserTodoRepository.GetCountByUserIdAsync(Context.User.Id);
        if (count >= 25)
        {
            await RespondErrorAsync("Too many todo items!",
                "You cannot create more than 25 todo list items, please remove some first!");
            return;
        }

        await _unitOfWork.UserTodoRepository.AddAsync(new UserTodo
        {
            UserId = Context.User.Id,
            Label = label,
            Description = description,
            Complete = false
        });
        await _unitOfWork.SaveChangesAsync();

        await RespondSuccessAsync($"Created todo item \"{label}\"");
    }

    [SlashCommand("view", "View your todo list")]
    public async Task View(
        [Description("Whether to post the todo list so that it is visible to other users.")]
        bool postPublicly = false)
    {
        IGuildUser? user = Context.User as IGuildUser;

        ButtonBuilder addButton = new("Add", "todo:addButton");
        ContainerBuilder container = new ContainerBuilder()
            .WithAccentColor(Colours.Info);
        
        IReadOnlyCollection<UserTodo> todos = await _unitOfWork.UserTodoRepository.GetByUserIdAsync(Context.User.Id);
        if (todos.Count == 0)
        {
            container.WithSection(new SectionBuilder()
                .WithTextDisplay($"""
                                  ## Todo List
                                  You do not have any todo list items.
                                  """)
                .WithAccessory(addButton)
            );
        }
        else
        {
            container.WithSection(new SectionBuilder()
                .WithTextDisplay($"""
                                  ## Todo list
                                  Here is your todo list {user?.Mention}
                                  """)
                .WithAccessory(addButton));
                
            List<UserTodo> pendingTodos = todos.Where(todo => !todo.Complete).ToList();
            if (pendingTodos.Count > 0)
            {
                container
                    .WithTextDisplay($"""
                                      ### 🟡 Pending Tasks
                                      """)
                    .WithSeparator(isDivider: false, spacing: SeparatorSpacingSize.Small)
                    .AddComponents(pendingTodos.SelectMany(RenderTodo).ToArray())
                    .WithSeparator(isDivider: false);
            }

            List<UserTodo> completedTodos = todos.Where(todo => todo.Complete).ToList();
            if (completedTodos.Count > 0)
            {
                container
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay($"""
                                      ### ✅ Completed Tasks
                                      """)
                    .WithSeparator(isDivider: false, spacing: SeparatorSpacingSize.Small)
                    .AddComponents(completedTodos.SelectMany(RenderTodo).ToArray());
            }
        }
        
        ComponentBuilderV2 builder = new ComponentBuilderV2().WithContainer(container);
        await RespondOrFollowUpAsync(components: builder.Build(), ephemeral: !postPublicly);
    }

    private IEnumerable<IMessageComponentBuilder> RenderTodo(UserTodo todo)
    {
        return
        [
            new SeparatorBuilder(),
            new TextDisplayBuilder($"""
                                    - **{todo.Label}**
                                    {todo.Description}
                                    """),
            new SeparatorBuilder(isDivider: false, spacing: SeparatorSpacingSize.Small),
            new ActionRowBuilder()
                .WithButton(
                    todo.Complete ? "Mark as Incomplete" : "Mark as Complete",
                    $"todo:completeButton_{todo.Id}",
                    ButtonStyle.Secondary)
                .WithButton("Remove", $"todo:removeButton_{todo.Id}", ButtonStyle.Danger),
            new SeparatorBuilder(isDivider: false, spacing: SeparatorSpacingSize.Small)
        ];
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

        await _unitOfWork.UserTodoRepository.CompleteByIdAsync(key);
        await _unitOfWork.SaveChangesAsync();

        await RespondSuccessAsync("Marked todo as complete");
    }

    [ComponentInteraction("todo:remove", true)]
    public async Task Remove(string id)
    {
        int key = GetId(id);

        await _unitOfWork.UserTodoRepository.DeleteByIdAsync(key);
        await _unitOfWork.SaveChangesAsync();

        await RespondSuccessAsync("Todo item removed");
    }

    private static int GetId(string id)
    {
        if (!int.TryParse(id, out int key)) throw new ArgumentOutOfRangeException(nameof(id));

        return key;
    }

    private async Task<MessageComponent?> BuildSelectMenuWithTodos(string customId)
    {
        IReadOnlyCollection<UserTodo> todos = await _unitOfWork.UserTodoRepository.GetByUserIdAsync(Context.User.Id);
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