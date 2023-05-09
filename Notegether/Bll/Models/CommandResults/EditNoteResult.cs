namespace Notegether.Bll.Models;

public record EditNoteResult(
    string ReadyAnswer,
    string MessageForOthers,
    List<long> OthersChatId
    );