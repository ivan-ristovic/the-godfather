using TheGodfather.Services;

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherServiceModule<TService> : TheGodfatherBaseModule where TService : ITheGodfatherService
    {
        protected TService _Service;


        protected TheGodfatherServiceModule(TService service, SharedData shared = null, DBService db = null)
            : base(shared, db)
        {
            _Service = service;
        }
    }
}
