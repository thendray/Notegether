namespace Notegether.Bll.Models;

public record NoteModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}