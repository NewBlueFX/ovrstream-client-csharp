using Newtonsoft.Json;

namespace ovrstream_client_csharp.Messages
{
    [JsonObject]
    internal class InvokeMethodMessage : OvrStreamMessage
    {
        [JsonProperty("type")]
        public override MessageTypes MessageType { get { return MessageTypes.InvokeMethod; } }

        [JsonProperty("method")]
        public int Method { get; set; }

        [JsonProperty("args")]
        public object[] Arguments { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }
    }
}
