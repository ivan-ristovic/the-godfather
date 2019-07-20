using TheGodfather.Services;

namespace TheGodfatherTests
{
    public interface ITheGodfatherServiceTest<T> where T : ITheGodfatherService
    {
        T Service { get; }


        void InitializeService();
    }
}
