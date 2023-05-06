using Microsoft.EntityFrameworkCore;
using Notegether.Bll.Models;
using Notegether.Dal.Entities;
using Notegether.Dal.Queries;
using Notegether.Dal.Queries.QueryResults;

namespace Notegether.Dal.Repositories;

public class NoteRepository : INoteRepository
{

    private readonly MyDbContext _dbContext;

    public NoteRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddNote(AddNoteQuery query)
    {
        await _dbContext.Notes.AddAsync(new NoteEntity
        {
           ShortIdentifier = query.Identifier,
           Title = query.Name,
           Description = query.Description,
           Text = query.Text,
           CreatorChatId = query.CreatorChatId,
        });

        await _dbContext.SaveChangesAsync();
    }
    public async Task<DeleteQueryResult> Delete(string identifier)
    {
        var noteForDell = await _dbContext.Notes.FirstOrDefaultAsync(x => x.ShortIdentifier == identifier);
        if (noteForDell != null)
        {
            _dbContext.Notes.Remove(noteForDell);

            await _dbContext.SaveChangesAsync();

            return new DeleteQueryResult(true, noteForDell.Title);
        }

        return new DeleteQueryResult(false, "");
    }
    public async Task<NoteEntity> Get(string identifier)
    {
        var noteForEditing = await 
            _dbContext.Notes.FirstOrDefaultAsync(x => x.ShortIdentifier == identifier);
        
        return noteForEditing;
    }
    public async Task Update(string identifier, NoteEntity newEntity)
    {
        var oldEntity = await _dbContext.Notes.FirstOrDefaultAsync(x => x.ShortIdentifier == newEntity.ShortIdentifier);

        if (oldEntity != null)
        {
            _dbContext.Entry(oldEntity).CurrentValues.SetValues(newEntity);
            await _dbContext.SaveChangesAsync();
        }
    }

}