using Notegether.Bll.Models;
using Notegether.Dal.Entities;
using Telegram.Bot.Types;

namespace Notegether.Bll.Services.Interfaces;

public interface INoteService
{

    public Task<string> CreateNoteWithIdentifier(NoteModel note, long chatId);

    public Task<string> DeleteNote(string identifier, long id);

    public Task<NoteModel> EditNote(string identifier, long id, string newData, string editPart);

    public IEnumerable<NoteEntity> GetMyNotes(long id);
    public Task<NoteEntity> GetOneNotes(string identifier);
}