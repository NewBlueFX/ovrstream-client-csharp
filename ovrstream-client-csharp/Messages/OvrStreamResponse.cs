using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ovrstream_client_csharp.Messages
{
    [JsonObject]
    internal class OvrStreamResponse
    {
        [JsonProperty("type")]
        public MessageTypes MessageType { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }
    }
}
