using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool.Test
{
    [TestClass]
    public class VideoConverterTest
    {
        private VideoConverter videoConverter;

        [TestInitialize]
        public void Initialise()
        {
            videoConverter = new VideoConverter();
        }

        [TestMethod]
        public async Task FfmpegTest()
        {
            string version = await videoConverter.FetchFfmpegVersion();
            Assert.AreEqual("2020-12-15-git-32586a42da-essentials_build-www.gyan.dev", version);
        }
    }
}
