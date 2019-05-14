using Newtonsoft.Json;
using System.Threading;

namespace ovrstream_client_csharp.Messages
{
    [JsonObject]
    internal abstract class OvrStreamMessage
    {
        private static long idCounter = 0;

        [JsonProperty("type")]
        public abstract MessageTypes MessageType { get; }

        [JsonProperty("id")]
        public long? Id { get; set; } = GetNextId();

        private static long GetNextId()
        {
            return Interlocked.Increment(ref idCounter);
        }
    }
}
