using System;
using TheGodfather.Database;
using TheGodfather.Services;

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherServiceModule<TService> : TheGodfatherModule where TService : ITheGodfatherService
    {
        protected TService Service { get; }

        // TODO DI inject?
        protected TheGodfatherServiceModule(TService service)
        {
            this.Service = service;
        }

        // TODO remove
        [Obsolete]
        protected TheGodfatherServiceModule(TService service, DbContextBuilder db)
            : base(db)
        {
            this.Service = service;
        }
    }
}
