using System.Collections.Generic;
using Newtonsoft.Json;

namespace PhonieCore.Mopidy
{
    public class SingleParamRequest : Request
    {
        public SingleParamRequest(string method, Dictionary<string, object> parameters)
        {
            this.Method = "core." + method;
            this.Parameters = parameters;
        }

        [JsonProperty("params")]
        public Dictionary<string, object> Parameters { get; set; }
    }
}
