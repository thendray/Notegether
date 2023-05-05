using Notegether.Bll.Models;
using Telegram.Bot.Types;

namespace Notegether.Bll.Services.Interfaces;

public interface INoteService
{

    public Task<string> CreateNoteWithIdentifier(NoteModel note, long chatId);

    public Task<string> DeleteNote(string identifier);

}