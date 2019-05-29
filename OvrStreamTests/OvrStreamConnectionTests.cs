using NUnit.Framework;
using ovrstream_client_csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class OvrStreamConnectionTests
    {
        public readonly Uri OvrStreamWebSocketAddress = new Uri("ws://127.0.0.1:8023");

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task DisconnectOnly()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task ConnectAndDisconnect()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);
            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task ConnectTwiceInARow()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);
            await connection.ConnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task UpdateVariableAndPlay()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("Name", DateTime.Now.ToString());
            await connection.UpdateVariablesAsync("Basic Follower Alert", variables, CancellationToken.None);

            await connection.ShowTitleAsync("Basic Follower Alert", "TestQueue", CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task UpdateVariableAndShowUpdateHide()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("Twitter_Username", "BEFORE");
            await connection.UpdateVariablesAsync("Basic BRB Screen", variables, CancellationToken.None);

            await connection.ShowTitleAsync("Basic BRB Screen", "TestQueue", CancellationToken.None);
            await Task.Delay(4000);

            variables.Clear();
            variables.Add("Twitter_Username", "AFTER");
            await connection.UpdateVariablesAsync("Basic BRB Screen", variables, CancellationToken.None);

            await Task.Delay(4000);
            await connection.HideTitleAsync("Basic BRB Screen", "TestQueue", CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task DeactivateAndActivateTitle()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            await connection.DeactivateTitleAsync("Basic BRB Screen", CancellationToken.None);
            await Task.Delay(2000);
            await connection.ActivateTitleAsync("Basic BRB Screen", CancellationToken.None);
            await Task.Delay(1000);

            var titles = await connection.GetTitlesAsync(CancellationToken.None);
            var title = titles.Single(t => t.Name.Equals("Basic BRB Screen", StringComparison.InvariantCultureIgnoreCase));
            await connection.DeactivateTitleAsync(title, CancellationToken.None);
            await Task.Delay(2000);
            await connection.ActivateTitleAsync(title, CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task OpenPlayout()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            await connection.OpenPlayoutAsync(CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task OpenTitleEdit()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            await connection.OpenTitleEditAsync("Basic BRB Screen", CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetCurrentVideoSettings()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            var settings = await connection.GetCurrentVideoSettingsAsync(CancellationToken.None);
            Assert.IsNotNull(settings);
            Assert.AreEqual(1920, settings.Width);
            Assert.AreEqual(1080, settings.Height);
            Assert.AreEqual(30, settings.FrameRate);
            Assert.AreEqual(false, settings.IsInterlaced);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetScenes()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            var scenes = await connection.GetScenesAsync(CancellationToken.None);
            Assert.IsNotNull(scenes);
            Assert.Greater(scenes.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetTitles()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            var titles = await connection.GetTitlesAsync(CancellationToken.None);
            Assert.IsNotNull(titles);
            Assert.Greater(titles.Length, 0);

            var imageBase64 = await connection.GetTitleIconAsync(titles[0], 114, 114, CancellationToken.None);
            Assert.IsNotNull(imageBase64);
            Assert.Greater(imageBase64.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetScenesWithXml()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            var sceneString = await connection.RunCommandXml("<newblue_ext command='getSceneList'/>", CancellationToken.None);
            Assert.IsNotNull(sceneString);
            Assert.Greater(sceneString.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task DownloadAndEncodeImage()
        {
            OvrStreamConnection connection = new OvrStreamConnection(OvrStreamWebSocketAddress);
            await connection.ConnectAsync(CancellationToken.None);

            var path = await connection.DownloadImageAsync(new Uri("https://pbs.twimg.com/profile_images/912736926125264896/0MYdjgqN_400x400.jpg"), CancellationToken.None);
            Assert.IsNotNull(path);
            Assert.Greater(path.Length, 0);

            var imageBase64 = await connection.EncodeImageAsync(path, CancellationToken.None);
            Assert.IsNotNull(imageBase64);
            Assert.Greater(imageBase64.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }
    }
}