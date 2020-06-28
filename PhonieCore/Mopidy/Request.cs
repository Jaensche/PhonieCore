using Newtonsoft.Json;

namespace PhonieCore.Mopidy
{
    public abstract class Request
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("id")]
        public string Id { get; set; } = "1";

        [JsonProperty("method")]
        public string Method { get; set; }        
    }
}
