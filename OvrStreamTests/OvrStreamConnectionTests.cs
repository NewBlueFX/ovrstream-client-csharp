using NUnit.Framework;
using ovrstream_client_csharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class OvrStreamConnectionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task ConnectAndDisconnect()
        {
            OvrStreamConnection connection = new OvrStreamConnection(8023);
            bool succeeded = await connection.ConnectAsync(CancellationToken.None);
            Assert.IsTrue(succeeded);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task UpdateVariableAndPlay()
        {
            OvrStreamConnection connection = new OvrStreamConnection(8023);
            bool succeeded = await connection.ConnectAsync(CancellationToken.None);
            Assert.IsTrue(succeeded);

            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("Name", DateTime.Now.ToString());
            await connection.UpdateVariablesAsync("Basic Follower Alert", "TestQueue", variables, CancellationToken.None);

            await connection.ShowTitleAsync("Basic Follower Alert", "TestQueue", CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task UpdateVariableAndShowUpdateHide()
        {
            OvrStreamConnection connection = new OvrStreamConnection(8023);
            bool succeeded = await connection.ConnectAsync(CancellationToken.None);
            Assert.IsTrue(succeeded);

            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("Twitter_Username", "BEFORE");
            await connection.UpdateVariablesAsync("Basic BRB Screen", "TestQueue", variables, CancellationToken.None);

            await connection.ShowTitleAsync("Basic BRB Screen", "TestQueue", CancellationToken.None);
            await Task.Delay(4000);

            variables.Clear();
            variables.Add("Twitter_Username", "AFTER");
            await connection.UpdateVariablesAsync("Basic BRB Screen", "TestQueue", variables, CancellationToken.None);

            await Task.Delay(4000);
            await connection.HideTitleAsync("Basic BRB Screen", "TestQueue", CancellationToken.None);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetScenes()
        {
            OvrStreamConnection connection = new OvrStreamConnection(8023);
            bool succeeded = await connection.ConnectAsync(CancellationToken.None);
            Assert.IsTrue(succeeded);

            var scenes = await connection.GetScenesAsync(CancellationToken.None);
            Assert.IsNotNull(scenes);
            Assert.Greater(scenes.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetTitles()
        {
            OvrStreamConnection connection = new OvrStreamConnection(8023);
            bool succeeded = await connection.ConnectAsync(CancellationToken.None);
            Assert.IsTrue(succeeded);

            var titles = await connection.GetTitlesAsync(CancellationToken.None);
            Assert.IsNotNull(titles);
            Assert.Greater(titles.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetScenesWithXml()
        {
            OvrStreamConnection connection = new OvrStreamConnection(8023);
            bool succeeded = await connection.ConnectAsync(CancellationToken.None);
            Assert.IsTrue(succeeded);

            var sceneString = await connection.RunCommandXml("<newblue_ext command='getSceneList'/>", CancellationToken.None);
            Assert.IsNotNull(sceneString);
            Assert.Greater(sceneString.Length, 0);

            await connection.DisconnectAsync(CancellationToken.None);
        }
    }
}