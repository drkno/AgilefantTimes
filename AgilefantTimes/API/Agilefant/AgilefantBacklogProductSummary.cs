using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Global

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantBacklogProductSummary : AgilefantBase
    {
        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("product")]
        public bool Product { get; private set; }

        [JsonProperty("standAlone")]
        public bool StandAlone { get; private set; }
    }
}
