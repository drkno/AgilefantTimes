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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SprintNumber { get; set; }

        [DataMember(IsRequired = false)]
        public bool DisplayUsercode { get; set; }

        [DataMember(IsRequired = false)]
        public bool DebugMode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string WebRoot { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Port { get; set; }

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

            if (string.IsNullOrWhiteSpace(config.WebRoot))
                config.WebRoot = Path.Combine(Environment.CurrentDirectory, "www");
            if (config.Port == 0)
                config.Port = 80;
            if (config.SprintNumber == 0)
                config.SprintNumber = -1;

            return config;
        }
    }
}