using Notegether.Dal.Entities;
using Notegether.Dal.Queries;
using Notegether.Dal.Queries.QueryResults;

namespace Notegether.Dal;

public interface INoteRepository
{
    public Task AddNote(AddNoteQuery query);
    public Task<DeleteQueryResult> Delete(string identifier);
    public Task<NoteEntity> Get(string identifier);
    public Task Update(string identifier, NoteEntity newEntity);
    public IEnumerable<NoteEntity> GetAllByCreatorId(long id);

    public NoteEntity GetLast();
}