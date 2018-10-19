using System;

namespace TheGodfather.Common
{
    public class UptimeInformation
    {
        public DateTimeOffset ProcessStartTime { get; private set; }
        public DateTimeOffset SocketStartTime { get; set; }


        public UptimeInformation(DateTimeOffset processStartTime)
        {
            this.ProcessStartTime = processStartTime;
            this.SocketStartTime = processStartTime;
        }


        public TimeSpan ProgramUptime => DateTimeOffset.Now - this.ProcessStartTime;
        public TimeSpan SocketUptime => DateTimeOffset.Now - this.SocketStartTime;
    }
}
