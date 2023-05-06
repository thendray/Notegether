﻿using Notegether.Bll.Models;
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
    public IEnumerable<NoteEntity> GetMyNotes(long id)
    {
        var notes = _noteRepository.GetAllByCreatorId(id);

        return notes;
    }
    public async Task<NoteEntity> GetOneNotes(string identifier)
    {
        return await _noteRepository.Get(identifier);
    }


    private string GenerateIdentifier()
    {
        var id = _noteRepository.GetLast().Id + 1;
        string identifier = "_" + id;

        return identifier;
    }
}