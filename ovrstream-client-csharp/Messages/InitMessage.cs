using Newtonsoft.Json;

namespace ovrstream_client_csharp.Messages
{
    [JsonObject]
    internal class InitMessage : OvrStreamMessage
    {
        [JsonProperty("type")]
        public override MessageTypes MessageType { get { return MessageTypes.Init; } }
    }
}
