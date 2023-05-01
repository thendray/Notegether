using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notegether;

public class Test
{
    private IConfiguration _configuration;

    public Test(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Print()
    {
        Console.WriteLine(_configuration.GetSection("Settings").Get<Settings>().Token);
    }
}