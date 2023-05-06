using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Queries;
using System;
using Notegether.Dal.Entities;
using Telegram.Bot.Types;

namespace Notegether.Bll.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;

    public NoteService(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<string> CreateNoteWithIdentifier(NoteModel note, long chatId)
    {

        string identifier = GenerateIdentifier(note.Name);
        
       await _noteRepository.AddNote(new AddNoteQuery(
            note.Name,
            note.Description,
            note.Text,
            identifier,
            chatId
        ));

        return identifier;
    }
    public async Task<string> DeleteNote(string identifier)
    {
        var result = await _noteRepository.Delete(identifier);

        if (result.IsFind)
        {
            return result.NoteName;
        }

        return "";
    }
    public async Task<NoteModel> EditNoteTitle(string identifier, string newData, string editPart)
    {
        NoteEntity result = await _noteRepository.Get(identifier);
        
        if (result != null)
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



    private string GenerateIdentifier(string name)
    {
        name = name.Replace(" ", "");

        if (name.Length <= 1)
        {
            name += "*";
            return "_" + name + name.Reverse();
        }
        
        if (name.Length <= 5)
        {
            return "_" + name;
        }
        else
        {
            return string.Concat("_", name.AsSpan(0, 5));
        }
    }
}