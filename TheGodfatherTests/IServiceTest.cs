namespace TheGodfatherTests
{
    public interface IServiceTest<T>
    {
        T Service { get; }


        void InitializeService();
    }
}
