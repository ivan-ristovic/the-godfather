namespace TheGodfatherTests.Services
{
    public interface IServiceTest<T>
    {
        T Service { get; }


        void InitializeService();
    }
}
