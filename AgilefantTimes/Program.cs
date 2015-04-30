using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AgilefantTimes.API;

namespace AgilefantTimes
{
    

    public static class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                var config = Config.Load("aftimes.conf");

                var session = AgilefantLogin.PerformLogin(config.Username, config.Password);
                var users = AgilefantUser.GetAgilefantUsers(ref session);
                var backlogs = AgilefantBacklog.GetAgilefantBacklogs(config.TeamNumber, ref session);
                var sprints = AgilefantSprint.GetAgilefantSprints(backlogs[0].Id, ref session);
                var sprintId = (from agilefantSprint in sprints
                    where agilefantSprint.Name.Contains(config.SprintNumber.ToString())
                    select agilefantSprint.Id).First();

                Console.WriteLine("{\"Hours\":[");
                for (int i = 0; i < users.Length; i++)
                {
                    var tasks = AgilefantTime.GetAgilefantTime(config.TeamNumber, backlogs[0].Id, sprintId,
                        users[i].Id, ref session);
                    Console.WriteLine("{{\"Name\":\"{0}\",\"StoryHours\":{1},\"TaskHours\":{2},\"TotalHours\":{3}}}" + (i+1 == users.Length ? "" : ","),
                        users[i].Name, tasks.StoryHours, tasks.TaskHours, tasks.TotalHours);
                }
                Console.WriteLine("]}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error occured at runtime:\n" + e.Message);
            }

#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}
