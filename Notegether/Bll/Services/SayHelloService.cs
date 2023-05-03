using Notegether.Dal;

namespace Notegether.Bll.Services;

public class SayHelloService : ISayHelloService
{

    private readonly INoteRepository _noteRepository;

    public SayHelloService(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }
    public string Hello()
    {
        _noteRepository.AddUser("Blabla");
        return "Hello";
    }
}