using System;

namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        private static readonly int RESET_TIME_S = 60;
        private bool _cooldown = false;
        private DateTimeOffset _resetTime;
        private readonly object _lock = new object();


        public TextReaction(int id, string trigger, string response, bool is_regex_trigger = false)
            : base(id, trigger, response, is_regex_trigger)
        {
            _resetTime = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(RESET_TIME_S);
        }


        public bool CanSend()
        {
            bool success = false;

            lock (_lock) {
                var now = DateTimeOffset.UtcNow;
                if (now >= _resetTime) {
                    _cooldown = false;
                    _resetTime = now + TimeSpan.FromSeconds(RESET_TIME_S);
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
