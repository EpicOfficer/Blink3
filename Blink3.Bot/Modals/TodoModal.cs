using Discord;
using Discord.Interactions;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Blink3.Bot.Modals;

// ReSharper disable once ClassNeverInstantiated.Global
public class TodoModal : IModal
{
    [ModalTextInput("todoModal:label", maxLength: 25)]
    [InputLabel("Label")]
    [RequiredInput]
    public required string Label { get; set; }

    [ModalTextInput("todoModal:description", maxLength: 50, style: TextInputStyle.Paragraph)]
    [InputLabel("Description")]
    [RequiredInput(false)]
    public string? Description { get; set; }

    public string Title => "Add todo list item";
}