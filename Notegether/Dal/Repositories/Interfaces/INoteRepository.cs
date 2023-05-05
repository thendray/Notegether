using Notegether.Dal.Queries;
using Notegether.Dal.Queries.QueryResults;

namespace Notegether.Dal;

public interface INoteRepository
{
    public Task AddNote(AddNoteQuery query);
    public Task<DeleteQueryResult> Delete(string identifier);
}