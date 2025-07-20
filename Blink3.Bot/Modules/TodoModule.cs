using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Bot.Modals;
using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[Group("todo", "Simple todo list")]
[UsedImplicitly]
public class TodoModule(IUnitOfWork unitOfWork, ILogger<TodoModule> logger) : BlinkModuleBase<IInteractionContext>(unitOfWork)
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
        UserLogContext userLogContext = new(Context.User);

        using (logger.BeginScope(new { User = userLogContext }))
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
            logger.LogInformation("{User} Created a todo list item", userLogContext);
        }
    }

    [SlashCommand("view", "View or edit your todo list")]
    public async Task View(
        [Description("Whether to post the todo list so that it is visible to other users.")]
        bool postPublicly = false)
    {
        UserLogContext userLogContext = new(Context.User);

        using (logger.BeginScope(new { User = userLogContext }))
        {
            IGuildUser? user = Context.User as IGuildUser;
            IReadOnlyCollection<UserTodo> todos = await _unitOfWork.UserTodoRepository.GetByUserIdAsync(Context.User.Id);

            ContainerBuilder builder = new ContainerBuilder()
                .WithAccentColor(Colours.Info)
                .WithSection(new SectionBuilder()
                    .WithTextDisplay($"## {(user is null ? "Your" : user.GetFriendlyName() + "'s")} todo list.")
                    .WithAccessory(new ButtonBuilder("Add", "todo:addButton")))
                .WithSeparator(isDivider: false);

            if (todos.Count == 0)
            {
                builder.WithTextDisplay("🔍 Your todo list is currently empty. Add a new item using the **Add Item** button above!");
            }

            foreach (UserTodo todo in todos)
            {
                string completionIcon = todo.Complete ? Icons.BoxChecked : Icons.Box;
                ButtonStyle style = todo.Complete ? ButtonStyle.Secondary : ButtonStyle.Success;
            
                builder.WithSeparator()
                    .WithTextDisplay($"""
                                      ### {completionIcon} {todo.Label}
                                      {todo.Description ?? "_ _"}
                                      """)
                    .WithSeparator(isDivider: false, spacing: SeparatorSpacingSize.Small)
                    .WithActionRow(new ActionRowBuilder()
                        .WithButton($"{(todo.Complete ? "Mark Incomplete" : "Mark Complete")}", 
                            $"todo:toggleComplete_{todo.Id}",
                            style)
                        .WithButton("Remove", $"todo:remove_{todo.Id}", ButtonStyle.Danger));
            }

            ComponentBuilderV2 components = new(builder);
            await RespondOrFollowUpAsync(components: components.Build(), ephemeral: !postPublicly);
            logger.LogInformation("{User} Viewed their todo list", userLogContext);
        }
    }

    [ComponentInteraction("todo:toggleComplete_*", true)]
    public async Task ToggleComplete(int id)
    {
        UserLogContext userLogContext = new(Context.User);

        using (logger.BeginScope(new { User = userLogContext }))
        {
            UserTodo? todo = await _unitOfWork.UserTodoRepository.GetByIdAsync(id);
            if (todo is null)
            {
                logger.LogInformation("{User} Tried to modify a todo list item that does not exist.", userLogContext);
                await RespondErrorAsync("Todo not found!", "I could not find that item on your todo list...");
                return;
            }
        
            if (Context.User.Id != todo.UserId)
            {
                logger.LogInformation("{User} Tried to modify a todo list item that does not belong to them.", userLogContext);
                await RespondErrorAsync("You cannot edit this item!", "Only the person who created this item can edit it.");
                return;
            }

            todo.Complete = !todo.Complete;

            await _unitOfWork.UserTodoRepository.UpdateAsync(todo);
            await _unitOfWork.SaveChangesAsync();

            await RespondSuccessAsync($"Marked todo as {(todo.Complete ? "complete" : "incomplete")}");
            logger.LogInformation("{User} Updated their todo list.", userLogContext);
        }
    }

    [ComponentInteraction("todo:remove_*", true)]
    public async Task Remove(int id)
    {
        UserLogContext userLogContext = new(Context.User);

        using (logger.BeginScope(new { User = userLogContext }))
        {
            UserTodo? todo = await _unitOfWork.UserTodoRepository.GetByIdAsync(id);
        
            if (todo is null)
            {
                logger.LogInformation("{User} Tried to remove a todo list item that does not exist.", userLogContext);
                await RespondErrorAsync("Todo not found!", "I could not find that item on your todo list...");
                return;
            }
        
            if (Context.User.Id != todo.UserId)
            {
                logger.LogInformation("{User} Tried to remove an item from somebody else's todo list.", userLogContext);
                await RespondErrorAsync("You cannot remove this item!", "Only the person who created this item can remove it.");
                return;
            }
        
            await _unitOfWork.UserTodoRepository.DeleteAsync(todo);
            await _unitOfWork.SaveChangesAsync();

            await RespondSuccessAsync("Todo item removed");
            logger.LogInformation("{User} Removed an item from their todo list.", userLogContext);
        }
    }
}