using Newtonsoft.Json;

namespace ovrstream_client_csharp.Messages
{
    [JsonObject]
    internal class IdleMessage : OvrStreamMessage
    {
        [JsonProperty("type")]
        public override MessageTypes MessageType { get { return MessageTypes.Idle; } }

        public IdleMessage()
        {
            this.Id = null;
        }
    }
}
