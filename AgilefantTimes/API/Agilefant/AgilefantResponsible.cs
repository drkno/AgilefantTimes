using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantResponsible : AgilefantBase
    {
        [JsonProperty("initials")]
        public string Initials { get; protected set; }
    }
}
