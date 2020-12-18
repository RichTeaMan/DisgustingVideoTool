using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool.Test
{
    [TestClass]
    public class YoutubePlaylistFactoryTest
    {
        private const string RESOURCE = "YoutubePlaylist.json";
        private const string RESOURCE_LINK = "PLH-huzMEgGWBUU5NcRJZ7Iss4nE3jfHh4";

        private YoutubePlaylistFactory factory;

        private string GetFileContents(string filename)
        {
            using (var resultStream = File.Open(filename, FileMode.Open))
            using (var reader = new StreamReader(resultStream, Encoding.UTF8))
            {
                var resultJson = reader.ReadToEnd();
                return resultJson;
            }
        }

        [TestInitialize]
        public void Initialise()
        {
            factory = new YoutubePlaylistFactory();
        }

        [TestMethod]
        public void ConvertPlaylist()
        {
            var contents = GetFileContents(RESOURCE);
            var playlist = factory.ConvertPlaylist(contents);
        }

        [TestMethod]
        public async Task DownloadPlaylist()
        {
            var playlist = await factory.DownloadPlaylist(RESOURCE_LINK);
            Assert.AreEqual(30, playlist.Videos.Length);
        }
    }
}
