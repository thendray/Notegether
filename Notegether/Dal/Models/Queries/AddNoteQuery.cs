namespace Notegether.Dal.Queries;

public record AddNoteQuery(
    string Name, 
    string Description,
    string Text,
    string Identifier,
    long CreatorChatId
    );