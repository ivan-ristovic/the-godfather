#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        private static readonly TimeSpan _cooldownTimeout = TimeSpan.FromMinutes(5);

        private bool cooldown;
        private DateTimeOffset resetTime;
        private object cooldownLock;


        public TextReaction(int id, string trigger, string response, bool isRegex = false)
            : base(id, trigger, response, isRegex)
        {
            this.Init();
        }

        public TextReaction(int id, string[] triggers, string response, bool isRegex = false)
            : base(id, triggers, response, isRegex)
        {
            this.Init();
        }


        public bool IsCooldownActive()
        {
            bool success = false;

            lock (this.cooldownLock) {
                var now = DateTimeOffset.UtcNow;
                if (now >= this.resetTime) {
                    this.cooldown = false;
                    this.resetTime = now + _cooldownTimeout;
                }
                
                if (!this.cooldown) {
                    this.cooldown = true;
                    success = true;
                }
            }
            
            return !success;
        }


        private void Init()
        {
            this.resetTime = DateTimeOffset.UtcNow + _cooldownTimeout;
            this.cooldownLock = new object();
            this.cooldown = false;
        }
    }
}
