using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant.Task
{
    public class AgilefantTaskHourEntry : AgilefantBase
    {
        public static async Task<AgilefantTaskHourEntry[]> GetEntriesBetween(int userId, DateTime start, DateTime end, AgilefantSession session)
        {
            var startDay = start.DayOfYear;
            var endDay = end.DayOfYear;
            var entries = new List<AgilefantTaskHourEntry>();
            for (var i = startDay; i <= endDay; i++)
            {
                var dayEntries = await GetEntriesForDay(userId, start.AddDays(i - startDay), session);
                entries.AddRange(dayEntries);
            }
            return entries.ToArray();
        }

        public static async Task<AgilefantTaskHourEntry[]> GetEntriesForDay(int userId, DateTime day,
            AgilefantSession session)
        {
            var url = string.Format("ajax/hourEntriesByUserAndDay.action?userId={0}&day={1}&year={2}", userId,
                day.DayOfYear, day.Year);
            var response = await session.Get(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AgilefantTaskHourEntry[]>(json);
        }

        [JsonProperty("date")]
        private long DateLong
        {
            set
            {
                Date = new DateTime(1970, 1, 1).AddMilliseconds(value);
            }
        }

        public DateTime Date { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("minutesSpent")]
        public int MinutesSpent { get; private set; }
        [JsonProperty("task")]
        public AgilefantTaskSummary Task { get; private set; }
    }

    public class AgilefantTaskSummary : AgilefantBacklogIterationSummary
    {
        [JsonProperty("iteration")]
        public AgilefantBacklogIterationSummary Iteration { get; private set; }
        [JsonProperty("story")]
        public AgilefantStorySummary Story { get; private set; }
    }

    public class AgilefantStorySummary : AgilefantBacklogIterationSummary
    {
        [JsonProperty("backlog")]
        public AgilefantBacklogIterationSummary Backlog { get; private set; }
    }

    public class AgilefantBacklogIterationSummary : AgilefantBase
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
    }

}
