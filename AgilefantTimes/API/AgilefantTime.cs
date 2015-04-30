using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AgilefantTimes.API.Common;

namespace AgilefantTimes.API
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
            get
            {
                return StoryHours + TaskHours;
            }
        }

        public double StoryHours
        {
            get
            {
                return Stories.Sum(story => story.Time);
            }
        }

        public double TaskHours
        {
            get
            {
                return Tasks.Sum(task => task.Time);
            }
        }

        public AgilefantElementTime[] Stories { get; protected set; }
        public AgilefantElementTime[] Tasks { get; protected set; }

        public class AgilefantElementTime
        {
            public AgilefantElementTime(double time, string description)
            {
                Description = description;
                Time = time;
            }

            public string Description { get; protected set; }
            public double Time { get; protected set; }
        }

        public static AgilefantTime GetAgilefantTime(int teamNumber, int backlogId, int sprintId, int userId, ref CookieContainer sessionCookies)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/generateTree.action");
            webRequest.AllowAutoRedirect = true;
            webRequest.CookieContainer = sessionCookies;
            webRequest.SetPostData("backlogSelectionType=0&productIds=" + teamNumber + "&projectIds=" + backlogId + "&iterationIds=" + sprintId + "&interval=NO_INTERVAL&startDate=&endDate=&userIds=" + userId);
            string htmlData = null;
            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                sessionCookies = webRequest.CookieContainer;
                var stream = webResponse.GetResponseStream();
                var streamReader = new StreamReader(stream);
                htmlData = streamReader.ReadToEnd();
                streamReader.Close();
            }
            return HtmlToAgilefantTime(htmlData);
        }

        private static AgilefantTime HtmlToAgilefantTime(string htmlData)
        {
            var lines = htmlData.Split('\n');
            var storyList = new List<AgilefantElementTime>();
            var taskList = new List<AgilefantElementTime>();
            var mode = true;
            var run = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("storyContainer")) {mode = true; continue;}
                if (lines[i].Contains("taskContainer")) {mode = false; continue;}
                if (lines[i].Contains("timesheet-content")) {run = true; continue;}
                if (lines[i].Contains("</ul>")) {run = false; continue;}
                if (!run) continue;

                if (lines[i].Contains("hoursum"))
                {
                    var hoursString = Regex.Match(lines[i], "(?<=(>)).*(?=(</))").Value;
                    var hoursSpl = hoursString.Split(new[]{" "}, StringSplitOptions.RemoveEmptyEntries);
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
    }
}
