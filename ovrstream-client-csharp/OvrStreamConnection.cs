using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ovrstream_client_csharp.Messages;
using ovrstream_client_csharp.Messages.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ovrstream_client_csharp
{
    public class OvrStreamConnection
    {
        private const int BufferSize = 1024;
        private const string SchedulerName = "scheduler";

        private ClientWebSocket m_WebSocket;
        private CancellationTokenSource m_ReadTokenSource;
        private readonly Dictionary<long, OvrStreamResponse> m_Responses = new Dictionary<long, OvrStreamResponse>();
        private readonly Dictionary<string, Dictionary<string, int>> m_ObjectMethods = new Dictionary<string, Dictionary<string, int>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The port used to connect to the OvrStream application
        /// </summary>
        public int Port { get; private set; }

        public OvrStreamConnection(int port)
        {
            this.Port = port;
        }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await DisconnectAsync(cancellationToken);

            m_WebSocket = new ClientWebSocket();
            await m_WebSocket.ConnectAsync(new Uri($"ws://127.0.0.1:{Port}"), cancellationToken);

            var _ = ReadAsync();

            var initResponse = await SendMessageAsync(new InitMessage(), cancellationToken);
            var dataObject = initResponse.Data as JObject;
            foreach (KeyValuePair<string, JToken> obj in dataObject)
            {
                m_ObjectMethods[obj.Key] = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

                foreach (var method in obj.Value["methods"])
                {
                    JArray methodArray = method as JArray;

                    string methodName = methodArray[0].Value<string>();
                    int key = methodArray[1].Value<int>();

                    m_ObjectMethods[obj.Key][methodName] = key;
                }
            }

            await SendMessageAsync(new IdleMessage(), cancellationToken);

            return true;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            m_ReadTokenSource?.Cancel();
            while (m_ReadTokenSource != null)
            {
                await Task.Delay(50, cancellationToken);
            }

            if (m_WebSocket != null)
            {
                try
                {
                    await m_WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", cancellationToken);
                }
                catch { }
                m_WebSocket = null;
            }
        }

        public async Task<Scene[]> GetScenesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sceneListResponse = await InvokeMethodAsync("scheduleCommandXml", new object[] { new GetSceneListCommand().ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(sceneListResponse.Data.Value<string>());
            List<Scene> sceneList = new List<Scene>();
            foreach(XmlElement sceneElement in responseDocument.SelectNodes("newblue_ext/scenes"))
            {
                sceneList.Add(Scene.Parse(sceneElement));
            }

            return sceneList.ToArray();
        }

        public async Task CloseSceneAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await InvokeMethodAsync("scheduleCommandXml", new object[] { new CloseSceneCommand().ToString() }, cancellationToken);
        }

        public Task OpenSceneAsync(Scene scene, CancellationToken cancellationToken)
        {
            return OpenSceneAsync(scene.Path, cancellationToken);
        }

        public async Task OpenSceneAsync(string scenePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            OpenSceneCommand command = new OpenSceneCommand
            {
                File = scenePath,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        public async Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();


            var titleListResponse = await InvokeMethodAsync("scheduleCommandXml", new object[] { new GetTitleControlInfoCommand().ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(titleListResponse.Data.Value<string>());
            List<Title> titleList = new List<Title>();
            foreach (XmlElement titleElement in responseDocument.SelectNodes("newblue_ext/title"))
            {
                titleList.Add(Title.Parse(titleElement));
            }

            return titleList.ToArray();
        }

        public Task ShowTitleAsync(Title title, CancellationToken cancellationToken)
        {
            return ShowTitleAsync(title.Id, title.Id, cancellationToken);
        }

        public Task ShowTitleAsync(Title title, string queue, CancellationToken cancellationToken)
        {
            return ShowTitleAsync(title.Id, queue, cancellationToken);
        }

        public Task ShowTitleAsync(string title, CancellationToken cancellationToken)
        {
            return ShowTitleAsync(title, title, cancellationToken);
        }

        public async Task ShowTitleAsync(string title, string queue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "animatein+override+duration",
                Id = title,
                Queue = queue,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        public Task HideTitleAsync(Title title, CancellationToken cancellationToken)
        {
            return HideTitleAsync(title.Id, title.Id, cancellationToken);
        }

        public Task HideTitleAsync(Title title, string queue, CancellationToken cancellationToken)
        {
            return HideTitleAsync(title.Id, queue, cancellationToken);
        }

        public Task HideTitleAsync(string title, CancellationToken cancellationToken)
        {
            return HideTitleAsync(title, title, cancellationToken);
        }

        public async Task HideTitleAsync(string title, string queue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "animateout+override",
                Id = title,
                Queue = queue,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        public Task UpdateVariablesAsync(Title title, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
        {
            return UpdateVariablesAsync(title.Id, title.Id, variables, cancellationToken);
        }

        public Task UpdateVariablesAsync(Title title, string queue, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
        {
            return UpdateVariablesAsync(title.Id, queue, variables, cancellationToken);
        }

        public Task UpdateVariablesAsync(string title, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
        {
            return UpdateVariablesAsync(title, title, variables, cancellationToken);
        }

        public async Task UpdateVariablesAsync(string title, string queue, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "update",
                Id = title,
                Queue = "Alert",
                Data = variables.Select(kvp => new XmlVariable { Name = kvp.Key, Value = kvp.Value }).ToArray(),
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        public async Task<string> RunCommandXml(string xml, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await InvokeMethodAsync("scheduleCommandXml", new object[] { xml }, cancellationToken);
            return response.Data.Value<string>();
        }

        private Task<OvrStreamResponse> InvokeMethodAsync(string method, object[] arguments, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!m_ObjectMethods[SchedulerName].ContainsKey(method))
            {
                throw new InvalidOperationException($"Unknown method on scheduler object: {method}");
            }

            var message = new InvokeMethodMessage
            {
                Object = SchedulerName,
                Method = m_ObjectMethods[SchedulerName][method],
                Arguments = arguments,
            };

            return SendMessageAsync(message, cancellationToken);
        }

        private async Task ReadAsync()
        {
            m_ReadTokenSource = new CancellationTokenSource();

            try
            {
                byte[] buffer = new byte[BufferSize];
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
                StringBuilder json = new StringBuilder(BufferSize);
                while (m_ReadTokenSource != null && !m_ReadTokenSource.IsCancellationRequested)
                {
                    var result = await m_WebSocket.ReceiveAsync(segment, m_ReadTokenSource.Token);
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            json.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            if (result.EndOfMessage)
                            {
                                HandlePacket(json.ToString());
                                json.Clear();
                            }
                            break;
                        case WebSocketMessageType.Close:
                            await DisconnectAsync(CancellationToken.None);
                            break;
                        case WebSocketMessageType.Binary:
                        default:
                            break;
                    }
                }
            }
            catch { }

            m_ReadTokenSource?.Dispose();
            m_ReadTokenSource = null;
        }

        private void HandlePacket(string json)
        {
            OvrStreamResponse response = JsonConvert.DeserializeObject<OvrStreamResponse>(json);
            if (response.Id.HasValue)
            {
                m_Responses[response.Id.Value] = response;
            }
            else
            {
                // TODO: Handle not id message
                throw new NotImplementedException("Handle not id message");
            }
        }

        private async Task<OvrStreamResponse> SendMessageAsync(OvrStreamMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string json = JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await m_WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);

            if (message.Id.HasValue)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (m_Responses.ContainsKey(message.Id.Value))
                    {
                        var response = m_Responses[message.Id.Value];
                        m_Responses.Remove(message.Id.Value);
                        return response;
                    }

                    await Task.Delay(50);
                }
            }

            return null;
        }
    }
}
