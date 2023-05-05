﻿using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services;
using Notegether.Bll.Services.Interfaces;
using Telegram.Bot.Types;

namespace Notegether.Bll.Commands;

public record CreateNoteCommand(
    long ChatId,
    string Name,
    string Description,
    string Text
    ) : IRequest<CreateNoteResult>;

public class CreateNoteCommandHandler : 
    IRequestHandler<CreateNoteCommand, CreateNoteResult>
{
    
    private readonly INoteService _service;

    public CreateNoteCommandHandler(INoteService service)
    {
        _service = service;
    }

    public async Task<CreateNoteResult> Handle(
        CreateNoteCommand request,
        CancellationToken cancellationToken)
    {

        var noteModel = new NoteModel
        {
            Name = request.Name,
            Description = request.Description,
            Text = request.Text
        };

        var result = await _service.CreateNoteWithIdentifier(noteModel, request.ChatId);

        return new CreateNoteResult(result);
    }
}