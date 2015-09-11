using System;
using System.Linq;
using System.Threading.Tasks;
using AgilefantTimes.API.Agilefant.Task;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantClient
    {
        [JsonProperty("session")]
        public AgilefantSession Session { get; private set; }

        /// <summary>
        /// Creates a new Agilefant Client
        /// </summary>
        /// <param name="session"></param>
        public AgilefantClient(AgilefantSession session)
        {
            Session = session;
        }

        /// <summary>
        /// Private constructor for deserialization.
        /// </summary>
        private AgilefantClient()
        {
        }

        /// <summary>
        /// Gets a list of sprints for the current user
        /// </summary>
        /// <returns></returns>
        public Task<AgilefantSprintSummary[]> GetSprintSummaries(int backlogId)
        {
            return AgilefantSprintSummary.GetSprints(backlogId, Session);
        }

        /// <summary>
        /// Gets detail about a specific sprint.
        /// </summary>
        /// <param name="sprintId">Id of the sprint to get.</param>
        /// <returns>Sprint details.</returns>
        public Task<AgilefantSprint> GetSprint(int sprintId)
        {
            return AgilefantSprint.GetSprint(sprintId, Session);
        }

        /// <summary>
        /// Gets detail about all avalible sprints.
        /// </summary>
        /// <param name="projectId">Id of the project to get sprints for.</param>
        /// <returns>Sprint details.</returns>
        public Task<AgilefantSprint[]> GetSprints(int projectId)
        {
            return AgilefantSprint.GetSprints(projectId, Session);
        }

        /// <summary>
        /// Gets a list of agilefant users
        /// </summary>
        /// <returns></returns>
        public Task<AgilefantUser[]> GetUsers()
        {
            return AgilefantUser.GetAgilefantUsers(Session);
        }

        /// <summary>
        /// Gets a list of the backlogs for a team
        /// </summary>
        /// <param name="teamNumber"></param>
        /// <returns></returns>
        public Task<AgilefantBacklog[]> GetBacklogs(int teamNumber)
        {
            return AgilefantBacklog.GetAgilefantBacklogs(teamNumber, Session);
        }

        /// <summary>
        /// Gets the time for a user from a team on a backlog for a specific sprint
        /// </summary>
        /// <param name="teamNumber">The team id</param>
        /// <param name="backlogId">The backlog id</param>
        /// <param name="sprintId">The sprint id</param>
        /// <param name="userId">The user id</param>
        /// <returns>The times for the user</returns>
        public Task<AgilefantTime> GetTime(int teamNumber, int backlogId, int sprintId, int userId)
        {
            return AgilefantTime.GetTimes(teamNumber, backlogId, sprintId, userId, Session);
        }

        /// <summary>
        /// Gets logged entries on a day for a user.
        /// </summary>
        /// <param name="userId">User to get.</param>
        /// <param name="day">Day to get.</param>
        /// <returns>Logged entries.</returns>
        public Task<AgilefantTaskHourEntry[]> GetLoggedTaskTime(int userId, DateTime day)
        {
            return AgilefantTaskHourEntry.GetEntriesForDay(userId, day, Session);
        }

        /// <summary>
        /// Gets logged entries between two days for a user.
        /// </summary>
        /// <param name="userId">User to get.</param>
        /// <param name="startDay">Start day to use.</param>
        /// <param name="endDay">End day to use.</param>
        /// <returns>Logged entries.</returns>
        public Task<AgilefantTaskHourEntry[]> GetLoggedTaskTime(int userId, DateTime startDay, DateTime endDay)
        {
            return AgilefantTaskHourEntry.GetEntriesBetween(userId, startDay, endDay, Session);
        }

        /// <summary>
        /// Gets all teams that are accessable.
        /// </summary>
        /// <returns>Avalible Agilefant teams.</returns>
        public Task<AgilefantTeam[]> GetTeams()
        {
            return AgilefantTeam.GetTeams(Session);
        }

        /// <summary>
        /// Gets a sprint based on current configuration.
        /// </summary>
        /// <param name="sprintNumber">Number of the sprint to get.</param>
        /// <param name="sprintPool">Pool of sprints to select from.</param>
        /// <returns>The requested sprint (or nearest guess).</returns>
        public static AgilefantSprintSummary SelectSprint(int sprintNumber, AgilefantSprintSummary[] sprintPool)
        {
            Array.Sort(sprintPool, (a, b) => a.StartDate.CompareTo(b.StartDate));

            if (sprintNumber >= 0)
            {
                foreach (var sprint in sprintPool.Where(sprint => sprint.Name.Contains(sprintNumber.ToString())))
                {
                    return sprint;
                }
                return sprintPool[sprintNumber];
            }

            var now = DateTime.Now.Date;
            var closest = sprintPool[0];
            foreach (var sprint in sprintPool)
            {
                if (sprint.StartDate <= now && sprint.EndDate >= now)
                {
                    closest = sprint;
                    break;
                }
                var diff = now < sprint.StartDate ? sprint.StartDate - now : now - sprint.EndDate;
                var currDiff = now < closest.StartDate ? closest.StartDate - now : now - closest.EndDate;
                if (diff < currDiff)
                {
                    closest = sprint;
                }
            }
            return closest;
        }
    }
}
