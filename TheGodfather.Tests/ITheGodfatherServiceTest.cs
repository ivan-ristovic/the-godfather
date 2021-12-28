using TheGodfather.Services;

namespace TheGodfather.Tests;

public interface ITheGodfatherServiceTest<T> where T : ITheGodfatherService
{
    T Service { get; }


    void InitializeService();
}