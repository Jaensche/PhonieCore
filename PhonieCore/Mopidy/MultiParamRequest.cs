using Newtonsoft.Json;
using System.Collections.Generic;

namespace PhonieCore.Mopidy
{
    public class MultiParamRequest : Request
    {
        public MultiParamRequest(string method, Dictionary<string, object[]> parameters)
        {
            this.Method = "core." + method;
            this.parameters = parameters;
        }

        [JsonProperty("params")]
        public Dictionary<string, object[]> parameters { get; set; }
    }
}
