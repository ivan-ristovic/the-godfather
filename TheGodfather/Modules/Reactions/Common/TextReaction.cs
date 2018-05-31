using System;

namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        private static readonly TimeSpan CooldownTimeout = TimeSpan.FromMinutes(5);
        private bool _cooldown = false;
        private DateTimeOffset _resetTime;
        private readonly object _lock = new object();


        public TextReaction(int id, string trigger, string response, bool is_regex_trigger = false)
            : base(id, trigger, response, is_regex_trigger)
        {
            _resetTime = DateTimeOffset.UtcNow + CooldownTimeout;
        }


        public bool CanSend()
        {
            bool success = false;

            lock (_lock) {
                var now = DateTimeOffset.UtcNow;
                if (now >= _resetTime) {
                    _cooldown = false;
                    _resetTime = now + CooldownTimeout;
                }
                
                if (!_cooldown) {
                    _cooldown = true;
                    success = true;
                }
            }
            
            return success;
        }
    }
}
