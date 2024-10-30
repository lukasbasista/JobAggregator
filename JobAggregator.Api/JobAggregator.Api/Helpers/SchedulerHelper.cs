namespace JobAggregator.Api.Helpers
{
    public static class SchedulerHelper
    {
        private static readonly Random _random = new Random();

        private static readonly List<(TimeSpan Start, TimeSpan End)> TimeWindows = new List<(TimeSpan, TimeSpan)>
        {
            (TimeSpan.FromHours(1), TimeSpan.FromHours(5)),
            (TimeSpan.FromHours(9), TimeSpan.FromHours(11)),
            (TimeSpan.FromHours(15), TimeSpan.FromHours(17)),
            (TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(20)), TimeSpan.FromHours(22).Add(TimeSpan.FromMinutes(15)))
        };

        public static DateTimeOffset GetNextRandomTime()
        {
            var now = DateTimeOffset.Now;

            foreach (var window in TimeWindows)
            {
                var windowStart = now.Date.Add(window.Start);
                var windowEnd = now.Date.Add(window.End);

                if (now > windowEnd) continue;

                DateTimeOffset scheduleTime;

                if (now <= windowStart)
                {
                    var randomMinutes = _random.Next(0, (int)(windowEnd - windowStart).TotalMinutes);
                    scheduleTime = windowStart.AddMinutes(randomMinutes);
                }
                else
                {
                    var tomorrow = now.Date.AddDays(1);
                    var firstWindow = TimeWindows.First();
                    var tomorrowWindowStart = tomorrow.Add(firstWindow.Start);
                    var tomorrowWindowEnd = tomorrow.Add(firstWindow.End);

                    var randomMinutesTomorrow = _random.Next(0, (int)(tomorrowWindowEnd - tomorrowWindowStart).TotalMinutes);
                    return tomorrowWindowStart.AddMinutes(randomMinutesTomorrow);
                }

                return scheduleTime;
            }

            var nextDay = now.Date.AddDays(1);
            var firstTimeWindow = TimeWindows.First();
            var nextWindowStart = nextDay.Add(firstTimeWindow.Start);
            var nextWindowEnd = nextDay.Add(firstTimeWindow.End);

            var randomMinutesNextDay = _random.Next(0, (int)(nextWindowEnd - nextWindowStart).TotalMinutes);
            return nextWindowStart.AddMinutes(randomMinutesNextDay);
        }
    }
}
