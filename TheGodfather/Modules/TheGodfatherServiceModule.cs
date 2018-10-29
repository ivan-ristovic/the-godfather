#region USING_DIRECTIVES
using TheGodfather.Database;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherServiceModule<TService> : TheGodfatherModule where TService : ITheGodfatherService
    {
        protected TService Service { get; }


        protected TheGodfatherServiceModule(TService service, SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.Service = service;
        }
    }
}
