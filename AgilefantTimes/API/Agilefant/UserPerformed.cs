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
        public AgilefantTaskHourEntry[] Tasks { get; private set; }
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

        public UserPerformed(int userId, string name, AgilefantTaskHourEntry[] tasks)
        {
            Array.Sort(tasks, (a,b) => a.Date.CompareTo(b.Date));

            Id = userId;
            Tasks = tasks;
            Name = name;

            HasUnitTested = tasks.Any(t => t.Description.Contains("#test"));
            HasAcceptanceTested = tasks.Any(t => t.Description.Contains("#manualtest"));
            HasImplemented = tasks.Any(t => t.Description.Contains("#implement"));
            HasRefactored = tasks.Any(t => t.Description.Contains("#refactor"));
            HasDocumented = tasks.Any(t => t.Description.Contains("#document"));
            HasPeerProgrammed = tasks.Any(t => t.Description.Contains("#peer") || t.Description.Contains("#pair"));
            HasDoneTeamChores = tasks.Any(t => t.Description.Contains("#chore"));

            var dailyHours = new List<double>();
            var currentPos = -1;
            var currentDay = -1;
            foreach (var task in tasks)
            {
                if (currentDay != task.Date.DayOfYear)
                {
                    dailyHours.Add(0);
                    currentPos++;
                    currentDay = task.Date.DayOfYear;
                }

                dailyHours[currentPos] += task.MinutesSpent/60.0;
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
