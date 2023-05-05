namespace Notegether.Dal.Entities;

public record NoteEntity
{
    public long Id { get; set; }
    public string ShortIdentifier { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long CreatorChatId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

}