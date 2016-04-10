using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantResponsible : AgilefantBase
    {
        private string _name;

        [JsonProperty("initials")]
        public string Initials { get; protected set; }

        public string Name
        {
            get
            {
                return string.IsNullOrWhiteSpace(_name) ? Initials : _name;
            }
            set { _name = value; }
        }
    }
}
