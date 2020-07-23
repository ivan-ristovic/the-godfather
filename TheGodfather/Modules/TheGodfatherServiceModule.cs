using TheGodfather.Database;
using TheGodfather.Services;

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherServiceModule<TService> : TheGodfatherModule where TService : ITheGodfatherService
    {
        protected TService Service { get; }


        protected TheGodfatherServiceModule(TService service, DbContextBuilder db)
            : base(db)
        {
            this.Service = service;
        }
    }
}
