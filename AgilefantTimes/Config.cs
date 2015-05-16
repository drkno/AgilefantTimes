#region

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

#endregion

namespace AgilefantTimes
{
    [DataContract]
    public class Config
    {
        [DataMember(IsRequired = false)]
        public string Username { get; set; }

        [DataMember(IsRequired = false)]
        public string Password { get; set; }

        [DataMember(IsRequired = false)]
        public int TeamNumber { get; set; }

        [DataMember(IsRequired = false)]
        public int SprintNumber { get; set; } = -1;

        [DataMember(IsRequired = false)]
        public bool DisplayUsercode { get; set; }

        [DataMember(IsRequired = false)]
        public bool DebugMode { get; set; }

        [DataMember(IsRequired = false)]
        public string WebRoot { get; set; } = Path.Combine(Environment.CurrentDirectory, "www");

        [DataMember(IsRequired = false)]
        public int Port { get; set; } = 80;

        public static bool TryLoad(string location, out Config config)
        {
            try
            {
                config = Load(location);
                return true;
            }
            catch (Exception)
            {
                config = null;
                return false;
            }
        }

        public static Config Load(string location)
        {
            if (!File.Exists(location))
            {
                throw new Exception("Missing configuration file at " + location);
            }
            var serializer = new DataContractJsonSerializer(typeof (Config));
            var fileSteam = new FileStream(location, FileMode.Open);
            var config = (Config) serializer.ReadObject(fileSteam);
            fileSteam.Close();
#if DEBUG
            config.DebugMode = true;
#endif
            return config;
        }
    }
}