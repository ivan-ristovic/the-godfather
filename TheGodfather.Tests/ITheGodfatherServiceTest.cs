namespace TheGodfather.Tests;

public interface ITheGodfatherServiceTest<out T> where T : ITheGodfatherService
{
    T Service { get; }


    void InitializeService();
}