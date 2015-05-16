#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantTime
    {
        private AgilefantTime(AgilefantElementTime[] stories, AgilefantElementTime[] tasks)
        {
            Stories = stories;
            Tasks = tasks;
        }

        public double TotalHours
        {
            get { return StoryHours + TaskHours; }
        }

        public double StoryHours
        {
            get { return Stories.Sum(story => story.Time); }
        }

        public double TaskHours
        {
            get { return Tasks.Sum(task => task.Time); }
        }

        public AgilefantElementTime[] Stories { get; protected set; }
        public AgilefantElementTime[] Tasks { get; protected set; }

        internal static async Task<AgilefantTime> GetTimes(int teamNumber, int backlogId, int sprintId, int userId,
            AgilefantSession session)
        {
            var data = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"backlogSelectionType", "0"},
                {"productIds", teamNumber.ToString()},
                {"projectIds", backlogId.ToString()},
                {"iterationIds", sprintId.ToString()},
                {"interval", "NO_INTERVAL"},
                {"startDate", ""},
                {"endDate", ""},
                {"userIds", userId.ToString()},
            });

            var response = await session.Post("generateTree.action", data);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return ParseHtmlToTimes(content);
        }

        /// <summary>
        /// Attempts to parse some html to retrieve times
        /// </summary>
        /// <param name="htmlData">The html containing the times</param>
        /// <returns>The time</returns>
        private static AgilefantTime ParseHtmlToTimes(string htmlData)
        {
            var lines = htmlData.Split('\n');
            var storyList = new List<AgilefantElementTime>();
            var taskList = new List<AgilefantElementTime>();
            var mode = true;
            var run = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("storyContainer"))
                {
                    run = true;
                    mode = true;
                    continue;
                }
                if (lines[i].Contains("taskContainer"))
                {
                    run = true;
                    mode = false;
                    continue;
                }
                if (lines[i].Contains("</ul>"))
                {
                    run = false;
                    continue;
                }
                if (!run) continue;

                if (lines[i].Contains("hoursum"))
                {
                    var hoursString = Regex.Match(lines[i], "(?<=(>)).*(?=(</))").Value;
                    var hoursSpl = hoursString.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    double time = 0;
                    foreach (var s in hoursSpl)
                    {
                        var match = Regex.Match(s, "^[0-9]+(?=(h$))");
                        if (match.Success)
                        {
                            time += int.Parse(match.Value);
                            continue;
                        }
                        match = Regex.Match(s, "^[0-9]+(?=(min$))");
                        if (match.Success)
                        {
                            time += (double.Parse(match.Value)/60.0);
                        }
                    }
                    i++;
                    var description = Regex.Match(lines[i], "(?<=(>)).*(?=(</))").Value;
                    var element = new AgilefantElementTime(time, description);
                    if (mode) storyList.Add(element);
                    else taskList.Add(element);
                }
            }

            return new AgilefantTime(storyList.ToArray(), taskList.ToArray());
        }

        /// <summary>
        /// A class representing a discrete piece of time logged
        /// </summary>
        public class AgilefantElementTime
        {
            public AgilefantElementTime(double time, string description)
            {
                Description = description;
                Time = time;
            }

            /// <summary>
            /// A description of the time
            /// </summary>
            public string Description { get; protected set; }

            /// <summary>
            /// The amount of time spent
            /// </summary>
            public double Time { get; protected set; }
        }
    }
}