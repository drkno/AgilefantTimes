#region

using System;
using System.Linq;
using AgilefantTimes.API;
using AgilefantTimes.Output;

#endregion

namespace AgilefantTimes
{
    public static class Program
    {
        private static readonly JsonPrinter JsonPrinter = new JsonPrinter();

        public static void Main()
        {
            try
            {
                var config = Config.Load("aftimes.conf");

                var session = AgilefantLogin.PerformLogin(config.Username, config.Password);
                var users = AgilefantUser.GetAgilefantUsers(ref session);
                var backlogs = AgilefantBacklog.GetAgilefantBacklogs(config.TeamNumber, ref session);
                var sprints = AgilefantSprint.GetAgilefantSprints(backlogs[0].Id, ref session);

                var sprintId = -1;
                var sprintName = "";
                if (config.SprintNumber < 0)
                {
                    var currentDate = DateTime.Now.Date;
                    foreach (var sprint in sprints.Where(sprint => sprint.StartDate <= currentDate && sprint.EndDate >= currentDate))
                    {
                        sprintId = sprint.Id;
                        sprintName = sprint.Name;
                        break;
                    }
                }
                
                if (sprintId < 0)
                {
                    if (config.SprintNumber <= 0) config.SprintNumber = 1;
                    sprintId = (from agilefantSprint in sprints
                                    where agilefantSprint.Name.Contains(config.SprintNumber.ToString())
                                    select agilefantSprint.Id).First();
                    sprintName = (from sprint in sprints
                        where sprint.Id == sprintId
                        select sprint.Name).First();
                }

                Console.WriteLine("{\n\t\"TeamName\":\"" + backlogs[0].Name + "\",");
                Console.WriteLine("\t\"Sprint\":\"" + sprintName + "\",");
                Console.WriteLine("\t\"Hours\":[");
                for (var i = 0; i < users.Length; i++)
                {
                    var tasks = AgilefantTime.GetAgilefantTime(config.TeamNumber, backlogs[0].Id, sprintId,
                        users[i].Id, ref session);

                    var json =
                        string.Format(
                            "{{\"Name\":\"{0}\",\"StoryHours\":{1},\"TaskHours\":{2},\"TotalHours\":{3}}}" +
                            (i + 1 == users.Length ? "" : ","),
                            (config.DisplayUsercode ? users[i].UserCode : users[i].Name), tasks.StoryHours, tasks.TaskHours, tasks.TotalHours);
                    JsonPrinter.WriteLine(json, 2);
                }
                Console.WriteLine("\t]\n}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error occured at runtime:\n" + e.Message);
#if DEBUG
                throw;
#endif
            }

#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}