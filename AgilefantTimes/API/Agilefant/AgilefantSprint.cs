using System;
using System.Drawing;
using System.Threading.Tasks;
using AgilefantTimes.API.Agilefant.Common;
using AgilefantTimes.API.Agilefant.Story;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantSprint : AgilefantBase
    {
        /// <summary>
        /// Gets details about a sprint with a specified ID.
        /// </summary>
        /// <param name="sprintId">ID of the sprint to get.</param>
        /// <param name="session">Agilefant login session to use.</param>
        /// <returns>Details of the specified sprint.</returns>
        public static async Task<AgilefantSprint> GetSprint(int sprintId, AgilefantSession session)
        {
            var url = string.Format("ajax/iterationData.action?iterationId={0}", sprintId);
            var response = await session.Get(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AgilefantSprint>(json);
        }

        /// <summary>
        /// Downloads the burndown image for a sprint.
        /// </summary>
        /// <param name="sprintId">ID of sprint to get burndown for.</param>
        /// <param name="session">Agilefant login session to use.</param>
        /// <returns>The burndown image.</returns>
        public static async Task<Image> GenerateBurndownImage(int sprintId, AgilefantSession session)
        {
            var url = string.Format("drawIterationBurndown.action?backlogId={0}&timeZoneOffset=720", sprintId);
            var response = await session.Get(url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }

        /// <summary>
        /// Gets the burndown image for this sprint.
        /// </summary>
        /// <param name="session">Agilefant session to use to get burndown image.</param>
        /// <returns>The burndown image.</returns>
        public async Task<Image> GetBurndownImage(AgilefantSession session)
        {
            return await GenerateBurndownImage(Id, session);
        }

        [JsonProperty("assignees")]
        public AgilefantResponsible[] Assignees { get; private set; }
        [JsonProperty("backlogSize")]
        public int BacklogSize { get; private set; }
        [JsonProperty("baselineLoad")]
        public object BaselineLoad { get; private set; }
        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("endDate")]
        private long EndDateLong { get; set; }
        public DateTime EndTime
        {
            get
            {
                return new DateTime(1970, 1, 1).AddMilliseconds(EndDateLong);
            }
        }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("product")]
        public bool Product { get; private set; }
        [JsonProperty("rankedStories")]
        public AgilefantStory[] RankedStories { get; private set; }
        [JsonProperty("readonlyToken")]
        public object ReadonlyToken { get; private set; }
        [JsonProperty("root")]
        public AgilefantBacklogProductSummary ProductSummary { get; private set; }
        [JsonProperty("scheduleStatus")]
        public string ScheduleStatus { get; private set; }
        [JsonProperty("standAlone")]
        public bool StandAlone { get; private set; }
        [JsonProperty("startDate")]
        private long StartDateLong { get; set; }
        public DateTime StartDate
        {
            get
            {
                return new DateTime(1970, 1, 1).AddMilliseconds(StartDateLong);
            }
        }
        [JsonProperty("tasks")]
        public Task[] Tasks { get; private set; }
    }
}
