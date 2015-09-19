using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgilefantTimes.API.Agilefant.Task;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant
{
    public class UserPerformed
    {
        [JsonProperty("success")]
        private bool _success = true;
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
        public bool HasFixed { get; private set; }
        public double TotalHours { get; private set; }
        public double AverageHours { get; private set; }
        public int Days { get; private set; }
        public double[] DailyHours { get; private set; }
        public double LongestDay { get; private set; }
        public double ShortestDay { get; private set; }
        public Dictionary<string, double> ProgrammedWithHours { get; private set; }
        public Dictionary<string, double> PerformedTasks { get; private set; } 
        public string UserCode { get; private set; }

        public UserPerformed(int userId, string userCode, string name, AgilefantTaskHourEntry[] tasks, int numDays)
        {
            Array.Sort(tasks, (a,b) => a.Date.CompareTo(b.Date));

            Id = userId;
            Tasks = tasks;
            Name = name;
            UserCode = userCode;

            PerformedTasks = new Dictionary<string, double>();
            ProgrammedWithHours = new Dictionary<string, double>();

            PerformedTasks["test"] = 0.0;
            PerformedTasks["testmanual"] = 0.0;
            PerformedTasks["implement"] = 0.0;
            PerformedTasks["refactor"] = 0.0;
            PerformedTasks["document"] = 0.0;
            PerformedTasks["pair"] = 0.0;
            PerformedTasks["chore"] = 0.0;
            PerformedTasks["fix"] = 0.0;
            foreach (var task in tasks)
            {
                var desc = task.Description?.ToLower() ?? "";
                var time = task.MinutesSpent/60.0;

                if (desc.Contains("#test "))
                    PerformedTasks["test"] = time + PerformedTasks["test"];

                if (desc.Contains("#testmanual"))
                    PerformedTasks["testmanual"] = time + PerformedTasks["testmanual"];

                if (desc.Contains("#testmanual"))
                    PerformedTasks["testmanual"] = time + PerformedTasks["testmanual"];

                if (desc.Contains("#implement"))
                    PerformedTasks["implement"] = time + PerformedTasks["implement"];

                if (desc.Contains("#refactor"))
                    PerformedTasks["refactor"] = time + PerformedTasks["refactor"];

                if (desc.Contains("#document"))
                    PerformedTasks["document"] = time + PerformedTasks["document"];

                if (desc.Contains("#chore"))
                    PerformedTasks["chore"] = time + PerformedTasks["chore"];

                if (desc.Contains("#fix"))
                    PerformedTasks["fix"] = time + PerformedTasks["fix"];

                if (desc.Contains("#pair"))
                {
                    PerformedTasks["pair"] = time + PerformedTasks["pair"];
                    var ind = desc.IndexOf("#pair[", StringComparison.Ordinal);
                    if (ind < 0) continue;
                    ind += 6;
                    var end = desc.IndexOf("]", ind, StringComparison.Ordinal);
                    var pairs = desc.Substring(ind, end - ind).Split(',');
                    foreach (var pair in pairs)
                    {
                        var p = pair.Trim();
                        if (p == userCode)
                        {
                            continue;
                        }
                        var curr = time + (ProgrammedWithHours.ContainsKey(p) ? ProgrammedWithHours[p] : 0);
                        ProgrammedWithHours[p] = curr;
                    }
                }
            }
            
            HasUnitTested = Math.Abs(PerformedTasks["test"]) > 0.0;
            HasAcceptanceTested = Math.Abs(PerformedTasks["testmanual"]) > 0.0;
            HasImplemented = Math.Abs(PerformedTasks["implement"]) > 0.0;
            HasRefactored = Math.Abs(PerformedTasks["refactor"]) > 0.0;
            HasDocumented = Math.Abs(PerformedTasks["document"]) > 0.0;
            HasPeerProgrammed = Math.Abs(PerformedTasks["pair"]) > 0.0;
            HasDoneTeamChores = Math.Abs(PerformedTasks["chore"]) > 0.0;
            HasFixed = Math.Abs(PerformedTasks["fix"]) > 0.0;

            if (tasks.Length > 0 && numDays > 0)
            {
                var dailyHours = new double[numDays];
                var minDay = tasks.Min(t => t.Date.DayOfYear);

                foreach (var task in tasks)
                {
                    var index = task.Date.DayOfYear - minDay;
                    if (index >= numDays)
                    {
                        // some work was logged after the sprint completed. assume it was on last day
                        index = numDays - 1;
                    }
                    dailyHours[index] += task.MinutesSpent/60.0;
                }
                TotalHours = dailyHours.Sum();
                AverageHours = dailyHours.Average();
                LongestDay = dailyHours.Max();
                ShortestDay = dailyHours.Min();
                Days = dailyHours.Length;
                DailyHours = dailyHours.ToArray();
            }
            else
            {
                TotalHours = 0;
                AverageHours = 0;
                LongestDay = 0;
                ShortestDay = 0;
                Days = 0;
                DailyHours = new double[numDays];
            }
        }
    }
}
