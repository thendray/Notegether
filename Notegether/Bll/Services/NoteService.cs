using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Queries;
using System;
using Notegether.Dal.Entities;
using Notegether.Dal.Models.Enums;
using Telegram.Bot.Types;

namespace Notegether.Bll.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly IPermissionRepository _permissionRepository;

    public NoteService(INoteRepository noteRepository, IPermissionRepository permissionRepository)
    {
        _noteRepository = noteRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<string> CreateNoteWithIdentifier(NoteModel note, long chatId)
    {
        string identifier = GenerateIdentifier();
            
        await _noteRepository.AddNote(new AddNoteQuery(
            note.Name,
            note.Description,
            note.Text,
            identifier,
            chatId
        ));

        return identifier;
    }
    public async Task<string> DeleteNote(string identifier, long id)
    {
        var note = await _noteRepository.Get(identifier);

        if (note.CreatorChatId != id)
        {
            return "";
        }
        
        var result = await _noteRepository.Delete(identifier);

        IEnumerable<PermissionEntity> permissions = _permissionRepository.GetAllByIdentifier(identifier);

        foreach (var permission in permissions)
        {
            _permissionRepository.Delete(permission.NoteIdentifier, permission.WhoGetChatId);
        }

        if (result.IsFind)
        {
            return result.NoteName;
        }

        return "";
    }
    public async Task<NoteModel> EditNote(string identifier, long id, string newData, string editPart)
    {
        NoteEntity result = await _noteRepository.Get(identifier);
        PermissionEntity permission = await _permissionRepository.Get(identifier, id);
        
        if (result != null && result.CreatorChatId == id ||
            (permission != null && permission.PermissionStatus == PermissionStatus.Redactor))
        {
            switch (editPart)
            {
                case "title":
                    result.Title = newData;
                    break;

                case "description":
                    result.Description = newData;
                    break;
                case "text":
                    result.Text = newData;
                    break;
            }
            
            await _noteRepository.Update(identifier, result);
            return new NoteModel
            {
                Name = result.Title,
                Description = result.Description,
                Text = result.Text
            };
        }

        return null;
    }
    public IEnumerable<NoteEntity> GetMyNotes(long id)
    {
        var notes = _noteRepository.GetAllByCreatorId(id);

        return notes;
    }
    public async Task<NoteEntity> GetOneNote(string identifier, long userId)
    {
       var note = await _noteRepository.Get(identifier);
       if (note != null && note.CreatorChatId == userId)
       {
           return note;
       }

       var permission = await _permissionRepository.Get(identifier, userId);
       
       if (permission != null && (permission.PermissionStatus == PermissionStatus.Reader
                                  || permission.PermissionStatus == PermissionStatus.Redactor))
       {
           return note;
       }

       return null;
    }

    public async Task<IEnumerable<NoteEntity>> GetOtherNotesReader(long userId)
    {
        var permissions = _permissionRepository.GetAllGotNotes(userId);
        List<NoteEntity> notes = new List<NoteEntity>();

        foreach (var permission in permissions)
        {
            if (permission.PermissionStatus == PermissionStatus.Reader)
            {
                notes.Add(await _noteRepository.Get(permission.NoteIdentifier));
            }
        }
        
        return notes;
    }
    
    
    public async Task<IEnumerable<NoteEntity>> GetOtherNotesRedactor(long userId)
    {
        var permissions = _permissionRepository.GetAllGotNotes(userId);
        List<NoteEntity> notes = new List<NoteEntity>();

        foreach (var permission in permissions)
        {
            if (permission.PermissionStatus == PermissionStatus.Redactor)
            {
                notes.Add(await _noteRepository.Get(permission.NoteIdentifier));
            }
        }
        
        return notes;
    }


    private string GenerateIdentifier()
    {
        var id = _noteRepository.GetLast().Id + 1;
        string identifier = "_" + id;

        return identifier;
    }
}