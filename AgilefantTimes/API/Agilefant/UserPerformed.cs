using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgilefantTimes.API.Agilefant.Task;

namespace AgilefantTimes.API.Agilefant
{
    public class UserPerformed
    {
        public AgilefantTaskHourEntry[] Tasks { get; internal set; }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool HasUnitTested { get; private set; }
        public bool HasAcceptanceTested { get; private set; }
        public bool HasImplemented { get; private set; }
        public bool HasRefactored { get; private set; }
        public bool HasDocumented { get; private set; }
        public bool HasPeerProgrammed { get; private set; }
        public bool HasDoneTeamChores { get; private set; }
        public double TotalHours { get; private set; }
        public double AverageHours { get; private set; }
        public int Days { get; private set; }
        public double[] DailyHours { get; private set; }
        public double LongestDay { get; private set; }
        public double ShortestDay { get; private set; }
        public Dictionary<string, double> ProgrammedWithHours { get; private set; } 
        public string UserCode { get; private set; }

        public UserPerformed(int userId, string userCode, string name, AgilefantTaskHourEntry[] tasks)
        {
            Array.Sort(tasks, (a,b) => a.Date.CompareTo(b.Date));

            Id = userId;
            Tasks = tasks;
            Name = name;
            UserCode = userCode;

            HasUnitTested = tasks.Any(t => t.Description.Contains("#test "));
            HasAcceptanceTested = tasks.Any(t => t.Description.Contains("#testmanual"));
            HasImplemented = tasks.Any(t => t.Description.Contains("#implement"));
            HasRefactored = tasks.Any(t => t.Description.Contains("#refactor"));
            HasDocumented = tasks.Any(t => t.Description.Contains("#document"));
            HasPeerProgrammed = tasks.Any(t => t.Description.Contains("#pair"));
            HasDoneTeamChores = tasks.Any(t => t.Description.Contains("#chore"));

            ProgrammedWithHours = new Dictionary<string, double>();

            var dailyHours = new List<double>();
            var maxDay = tasks.Max(t => t.Date.DayOfYear);
            var minDay = tasks.Min(t => t.Date.DayOfYear);
            var daysIntoSprint = maxDay - minDay;
            for (var i = 0; i <= daysIntoSprint; i++)
            {
                dailyHours.Add(0);
            }

            foreach (var task in tasks)
            {
                var index = task.Date.DayOfYear - minDay;
                dailyHours[index] += task.MinutesSpent/60.0;

                var ind = task.Description.IndexOf("#pair[", StringComparison.Ordinal);
                if (ind >= 0)
                {
                    ind += 6;
                    var end = task.Description.IndexOf("]", ind, StringComparison.Ordinal);
                    var pairs = task.Description.Substring(ind, end - ind).Split(',');
                    foreach (var pair in pairs)
                    {
                        var p = pair.Trim();
                        if (p == userCode)
                        {
                            continue;
                        }

                        if (ProgrammedWithHours.ContainsKey(p))
                        {
                            ProgrammedWithHours[p] += task.MinutesSpent / 60.0;
                        }
                        else
                        {
                            ProgrammedWithHours[p] = task.MinutesSpent / 60.0;
                        }
                    }
                }
            }
            TotalHours = dailyHours.Sum();
            AverageHours = dailyHours.Average();
            LongestDay = dailyHours.Max();
            ShortestDay = dailyHours.Min();
            Days = dailyHours.Count;
            DailyHours = dailyHours.ToArray();
        }
    }
}
