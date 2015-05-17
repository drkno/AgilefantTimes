using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantTeam
    {
        public AgilefantTeam(string name, string description, int id, AgilefantResponsible[] members, Sprint[] sprints)
        {
            Name = name;
            Description = description;
            Id = id;
            Members = members;
            Sprints = sprints;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Id { get; private set; }
        public AgilefantResponsible[] Members { get; private set; }
        public Sprint[] Sprints { get; private set; }

        public static async Task<AgilefantTeam[]> GetTeams(AgilefantSession session)
        {
            var response = await session.Get("ajax/retrieveAllProductsWithStandalone.action");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var summaries = JsonConvert.DeserializeObject<AgilefantBacklogProductSummary[]>(json);
            var teams = new List<AgilefantTeam>();
            foreach (var summary in summaries)
            {
                if (!summary.Name.Contains(":")) continue;
                var sprintData = await AgilefantSprint.GetSprints(summary.Id + 6, session);

                var sprints = new List<Sprint>();
                foreach (var sprint in sprintData)
                {
                    var s = new Sprint(sprint.Name, sprint.Description, sprint.Id, sprint.EndTime, sprint.StartDate);
                    sprints.Add(s);
                }

                teams.Add(new AgilefantTeam(summary.Name.Substring(2), summary.Description, summary.Id, sprintData[0].Assignees, sprints.ToArray()));
            }
            return teams.ToArray();
        }

        public class Sprint
        {
            public Sprint(string name, string description, int id, DateTime startDate, DateTime endDate)
            {
                Name = name;
                Description = description;
                Id = id;
                StartDate = startDate;
                EndDate = endDate;
            }

            public string Name { get; private set; }
            public string Description { get; private set; }
            public int Id { get; private set; }
            public DateTime StartDate { get; private set; }
            public DateTime EndDate { get; private set; }
        }
    }
}
