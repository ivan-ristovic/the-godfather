using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Modules
{
    public abstract class GodfatherBaseModule : BaseExtension
    {
        protected SharedData SharedData;
        protected DatabaseService DatabaseService;


        protected GodfatherBaseModule(SharedData shared, DatabaseService db)
        {
            SharedData = shared;
            DatabaseService = db;
        }

        /*
        in modules add:
        
        public module(SharedData shared, DatabaseService db) : base(shared, db) { }
        */

        protected override void Setup(DiscordClient client)
        {

        }
    }
}
