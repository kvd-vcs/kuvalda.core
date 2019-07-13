using System;

namespace Kuvalda.Core
{
    public class NowDateTimeService : INowDateTimeService
    {
        public DateTime GetNow()
        {
            return DateTime.UtcNow;
        }
    }
}