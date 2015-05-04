using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgilefantTimes.API;

namespace AgilefantTimes.Output
{
    [DataContract]
    public class JsonOutputTime
    {
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public double StoryHours { get; private set; }
        [DataMember]
        public double TaskHours { get; private set; }
        [DataMember]
        public double TotalHours { get; private set; }

        /// <summary>
        /// Used for storing time such that it can be output as a JSON object.
        /// </summary>
        /// <param name="name">Person name to use.</param>
        /// <param name="tasks">Tasks performed by the person.</param>
        public JsonOutputTime(string name, AgilefantTime tasks)
        {
            Name = name;
            StoryHours = tasks.StoryHours;
            TaskHours = tasks.TaskHours;
            TotalHours = tasks.TotalHours;
        }
    }
}
