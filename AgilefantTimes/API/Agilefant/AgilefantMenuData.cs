using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantMenuData
    {
        [JsonProperty("addClass")]
        public string AddClass { get; protected set; }
        [JsonProperty("children")]
        public AgilefantMenuData[] Children { get; protected set; }
        [JsonProperty("expand")]
        public bool Expand { get; protected set; }
        [JsonProperty("icon")]
        public bool Icon { get; protected set; }
        [JsonProperty("id")]
        public int Id { get; protected set; }
        [JsonProperty("scheduleStatus")]
        public string ScheduleStatus { get; protected set; }
        [JsonProperty("title")]
        public string Title { get; protected set; }

        internal static async Task<AgilefantMenuData[]> GetMenuData(AgilefantSession session)
        {
            var response = await session.Get("ajax/menuData.action");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AgilefantMenuData[]>(json);
        }
    }
}
