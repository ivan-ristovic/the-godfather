using TheGodfather.Services;

namespace TheGodfather.Modules
{
    public class TheGodfatherServiceModule<TService> : TheGodfatherBaseModule where TService : IGodfatherService
    {
        protected TService Service;


        protected TheGodfatherServiceModule(TService service, SharedData shared = null, DatabaseService db = null)
            : base(shared, db)
        {
            Service = service;
        }
    }
}
