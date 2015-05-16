#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;

#endregion

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantSprintSummary : AgilefantBase
    {
        [JsonProperty("backlogSize")]
        public int BacklogSize { get; private set; }

        [JsonProperty("baselineLoad")]
        public object BaselineLoad { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("endDate")]
        protected long EndDateLong { get; private set; }

        public DateTime EndDate
        {
            get { return new DateTime(1970, 1, 1).AddMilliseconds(EndDateLong); }
        }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("product")]
        public bool Product { get; private set; }

        [JsonProperty("readonlyToken")]
        public object ReadonlyToken { get; private set; }

        [JsonProperty("root")]
        public AgilefantBacklogProductSummary ProductSummary { get; private set; }

        [JsonProperty("standAlone")]
        public bool StandAlone { get; private set; }

        [JsonProperty("startDate")]
        protected long StartDateLong { get; private set; }

        public DateTime StartDate
        {
            get { return new DateTime(1970, 1, 1).AddMilliseconds(StartDateLong); }
        }

        /// <summary>
        /// Gets an array of sprints available 
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="backlogId">The backlog to get sprints for.</param>
        /// <returns>The sprint</returns>
        internal static async Task<AgilefantSprintSummary[]> GetSprints(int backlogId, AgilefantSession session)
        {
            var response = await session.Post("ajax/retrieveSubBacklogs.action", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"backlogId", backlogId.ToString()}
            }));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AgilefantSprintSummary[]>(json);
        }
    }
}