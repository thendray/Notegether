﻿using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Notegether.Dal.Entities;
using Telegram.Bot.Types;

namespace Notegether.Bll.Services.Interfaces;

public interface INoteService
{

    public Task<string> CreateNoteWithIdentifier(NoteModel note, long chatId);

    public Task<string> DeleteNote(string identifier, long id);

    public Task<NoteModel> EditNote(string identifier, long id, string newData, string editPart, EditType editType);

    public IEnumerable<NoteEntity> GetMyNotes(long id);
    public Task<NoteEntity> GetOneNote(string identifier, long userId);
    Task<IEnumerable<NoteEntity>> GetOtherNotesReader(long requestUserId);
    Task<IEnumerable<NoteEntity>> GetOtherNotesRedactor(long requestUserId);
    public List<long> GetChatIdsWithPermissions(string requestIdentifier, long requestUserId);
}