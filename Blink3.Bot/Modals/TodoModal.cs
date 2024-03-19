using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modals;

// ReSharper disable once ClassNeverInstantiated.Global
public class TodoModal : IModal
{
    public string Title => "Add todo list item";
    
    [ModalTextInput("todoModal:label", maxLength: 25)]
    [InputLabel("Label")]
    [RequiredInput]
    public required string Label { get; set; }
    
    [ModalTextInput("todoModal:description", maxLength: 50, style: TextInputStyle.Paragraph)]
    [InputLabel("Description")]
    [RequiredInput(false)]
    public string? Description { get; set; }
}