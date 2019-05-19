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

        public event EventHandler<EventArgs> OnDisconnected;

        /// <summary>
        /// The adddess used to connect to the OvrStream application
        /// </summary>
        public Uri Address { get; private set; }

        /// <summary>
        /// This constructor is used to create a new OvrStream connection.
        /// </summary>
        /// <param name="address">The websocket address to use.  Generally, this is ws://127.0.0.1:8023.</param>
        public OvrStreamConnection(Uri address)
        {
            this.Address = address;
        }

        /// <summary>
        /// Initiates a connect to the OvrStream websocket.  If OvrStream is not running, this will
        /// throw a <see cref="WebSocketException"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ensure we aren't currently connected
            await DisconnectAsync(cancellationToken);

            // Open the web socket
            m_WebSocket = new ClientWebSocket();
            await m_WebSocket.ConnectAsync(Address, cancellationToken);

            // Start the background thread to read the incoming data
            var _ = ReadAsync();

            // Send a message to the server to initialize the connection
            var initResponse = await SendMessageAsync(new InitMessage(), cancellationToken);

            // This message has a list of objects, which in turn have lists of methods avaiable to call
            // We process this data into a dictionary for use later when we call "InvokeMethodAsync".
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

            // Once we have finished initializing, we send an idle message to the server
            await SendMessageAsync(new IdleMessage(), cancellationToken);
        }

        /// <summary>
        /// This method forces a disconnect from the OvrStream websocket server.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Trigger the read thread to stop and wait for it to exit
            m_ReadTokenSource?.Cancel();
            while (m_ReadTokenSource != null)
            {
                await Task.Delay(50, cancellationToken);
            }

            // Close out the web socket
            if (m_WebSocket != null)
            {
                try
                {
                    await m_WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", cancellationToken);
                }
                catch { }
                m_WebSocket = null;

                await Task.Run(() => { OnDisconnected?.Invoke(this, null); });
            }
        }

        /// <summary>
        /// This method will bring OvrStream to the foreground.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task OpenPlayoutAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await InvokeMethodAsync("scheduleCommandXml", new object[] { new OpenPlayoutCommand().ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method will retrieve the current OvrStream video settings.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>The current OvrStream <see cref="VideoSettings"/>.</returns>
        public async Task<VideoSettings> GetCurrentVideoSettingsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await InvokeMethodAsync("scheduleCommandXml", new object[] { new GetCurrentVideoSettingsCommand().ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(response.Data.Value<string>());
            return VideoSettings.Parse(responseDocument.SelectSingleNode("newblue_ext") as XmlElement);
        }

        /// <summary>
        /// This method requests a list of the scenes available from OvrStream.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>An array of available <see cref="Scene"/>s.</returns>
        public async Task<Scene[]> GetScenesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sceneListResponse = await InvokeMethodAsync("scheduleCommandXml", new object[] { new GetSceneListCommand().ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(sceneListResponse.Data.Value<string>());
            List<Scene> sceneList = new List<Scene>();
            foreach (XmlElement sceneElement in responseDocument.SelectNodes("newblue_ext/scenes"))
            {
                sceneList.Add(Scene.Parse(sceneElement));
            }

            return sceneList.ToArray();
        }

        /// <summary>
        /// This method will save and close the current scene that is open in OvrStream.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task CloseSceneAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await InvokeMethodAsync("scheduleCommandXml", new object[] { new CloseSceneCommand().ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method will open a new scene in OvrStream.  It is a good idea to call the <see cref="CloseSceneAsync(CancellationToken)"/> method first.
        /// </summary>
        /// <param name="scene">The scene you wish to open.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task OpenSceneAsync(Scene scene, CancellationToken cancellationToken)
        {
            return OpenSceneAsync(scene.Path, cancellationToken);
        }

        /// <summary>
        /// This method will open a new scene in OvrStream.  It is a good idea to call the <see cref="CloseSceneAsync(CancellationToken)"/> method first.
        /// </summary>
        /// <param name="scenePath">The path to the scene you wish to open.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task OpenSceneAsync(string scenePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            OpenSceneCommand command = new OpenSceneCommand
            {
                File = scenePath,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method will get a list of titles currently available in the open scene.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>An array of <see cref="Title"/>s for the current scene.</returns>
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

        /// <summary>
        /// This method will tell OvrStream to start showing the title provided.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to show.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task ShowTitleAsync(Title title, CancellationToken cancellationToken)
        {
            return ShowTitleAsync(title.Id, title.Id, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to start showing the title provided.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to show.</param>
        /// <param name="queue">The queue to use for this action.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task ShowTitleAsync(Title title, string queue, CancellationToken cancellationToken)
        {
            return ShowTitleAsync(title.Id, queue, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to start showing the title provided.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to show.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task ShowTitleAsync(string titleId, CancellationToken cancellationToken)
        {
            return ShowTitleAsync(titleId, titleId, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to start showing the title provided.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to show.</param>
        /// <param name="queue">The queue to use for this action.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task ShowTitleAsync(string titleId, string queue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "animatein+override+duration",
                Id = titleId,
                Queue = queue,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to begin hiding the title provided.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to hide.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task HideTitleAsync(Title title, CancellationToken cancellationToken)
        {
            return HideTitleAsync(title.Id, title.Id, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to begin hiding the title provided.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to hide.</param>
        /// <param name="queue">The queue to use for this action.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task HideTitleAsync(Title title, string queue, CancellationToken cancellationToken)
        {
            return HideTitleAsync(title.Id, queue, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to begin hiding the title provided.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to hide.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task HideTitleAsync(string titleId, CancellationToken cancellationToken)
        {
            return HideTitleAsync(titleId, titleId, cancellationToken);
        }

        /// <summary>
        /// This method will tell OvrStream to begin hiding the title provided.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to hide.</param>
        /// <param name="queue">The queue to use for this action.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task HideTitleAsync(string titleId, string queue, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "animateout+override",
                Id = titleId,
                Queue = queue,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method will open the title provided in the OvrStream title editor.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to edit.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task OpenTitleEditAsync(Title title, CancellationToken cancellationToken)
        {
            return OpenTitleEditAsync(title.Id, cancellationToken);
        }

        /// <summary>
        /// This method will open the title provided in the OvrStream title editor.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to edit.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task OpenTitleEditAsync(string titleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            OpenTitleEditCommand command = new OpenTitleEditCommand
            {
                Title = titleId,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method sends a list of variable updates to the title provided.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to update.</param>
        /// <param name="variables">A dictionary of variables with the key begin the variable name.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public Task UpdateVariablesAsync(Title title, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
        {
            return UpdateVariablesAsync(title.Id, variables, cancellationToken);
        }

        /// <summary>
        /// This method sends a list of variable updates to the title provided.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to update.</param>
        /// <param name="variables">A dictionary of variables with the key begin the variable name.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        public async Task UpdateVariablesAsync(string titleId, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "update",
                Id = titleId,
                Queue = "Alert",
                Data = variables.Select(kvp => new XmlVariable { Name = kvp.Key, Value = kvp.Value }).ToArray(),
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);
        }

        /// <summary>
        /// This method downloads the image at the specified Uri.
        /// </summary>
        /// <param name="uri">The path the image to download.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>The local machine path to the downloaded image.</returns>
        public async Task<string> DownloadImageAsync(Uri uri, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DownloadImageCommand command = new DownloadImageCommand
            {
                Uri = uri,
            };

            var response = await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(response.Data.Value<string>());
            XmlElement root = responseDocument.SelectSingleNode("//newblue_ext") as XmlElement;
            if (root != null && root.HasAttribute("path"))
            {
                return root.GetAttribute("path");
            }

            return null;
        }

        /// <summary>
        /// This method will encode an local image file into a base64 encoded image.
        /// </summary>
        /// <param name="path">The local machine path to an image.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>The base64 encoded image data.</returns>
        public async Task<string> EncodeImageAsync(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EncodeImageCommand command = new EncodeImageCommand
            {
                Path = path,
            };

            var response = await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(response.Data.Value<string>());
            XmlElement image = responseDocument.SelectSingleNode("//newblue_ext/image") as XmlElement;
            if (image != null)
            {
                return image.InnerText;
            }

            return null;
        }

        /// <summary>
        /// This method will look up a specific title's icon.
        /// </summary>
        /// <param name="title">The <see cref="Title"/> to retrieve the icon for.</param>
        /// <param name="width">The desired width of the icon.</param>
        /// <param name="height">The desired height of the icon.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>The base64 encoded image data.</returns>
        public Task<string> GetTitleIconAsync(Title title, int width, int height, CancellationToken cancellationToken)
        {
            return GetTitleIconAsync(title.Id, width, height, cancellationToken);
        }

        /// <summary>
        /// This method will look up a specific title's icon.
        /// </summary>
        /// <param name="titleId">The ID <see cref="Title"/> to retrieve the icon for.</param>
        /// <param name="width">The desired width of the icon.</param>
        /// <param name="height">The desired height of the icon.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>The base64 encoded image data.</returns>
        public async Task<string> GetTitleIconAsync(string titleId, int width, int height, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GetTitleIconCommand command = new GetTitleIconCommand
            {
                Title = titleId,
                Width = width,
                Height = height,
            };

            var response = await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() }, cancellationToken);

            XmlDocument responseDocument = new XmlDocument();
            responseDocument.LoadXml(response.Data.Value<string>());
            XmlElement image = responseDocument.SelectSingleNode("//newblue_ext/image") as XmlElement;
            if (image != null)
            {
                return image.InnerText;
            }

            return null;
        }

        /// <summary>
        /// This method can be used to run any OvrStream XML command.
        /// </summary>
        /// <param name="xml">The XML command data.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>The XML response.</returns>
        public async Task<string> RunCommandXml(string xml, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await InvokeMethodAsync("scheduleCommandXml", new object[] { xml }, cancellationToken);
            return response.Data.Value<string>();
        }

        #region Private Methods
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
                // Allocate the buffer once and read
                byte[] buffer = new byte[BufferSize];
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
                StringBuilder json = new StringBuilder(BufferSize);

                while (m_WebSocket.State == WebSocketState.Open && !m_ReadTokenSource.IsCancellationRequested)
                {
                    // Read
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
                            throw new InvalidOperationException("Cannot process binary websocket messages.");
                    }
                }
            }
            catch { }

            m_ReadTokenSource?.Dispose();
            m_ReadTokenSource = null;

            await this.DisconnectAsync(CancellationToken.None);
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
            }
        }

        private async Task<OvrStreamResponse> SendMessageAsync(OvrStreamMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string json = JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await m_WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);

            // If the message has an Id field, we need to wait for the response.
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
        #endregion
    }
}
