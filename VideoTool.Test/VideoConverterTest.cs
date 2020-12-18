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

        [TestMethod]
        public async Task ConvertTest()
        {
            string fileName = "sample";
            string input = fileName + ".mkv";
            string resultFile = fileName + ".mp4";
            // remove old runs
            File.Delete(resultFile);
            File.Delete(fileName + ".converted.mp4");
            File.Delete("backup" + input);

            await videoConverter.ConvertVideo(input);

            Assert.IsTrue(File.Exists(resultFile), "mp4 file does not exist.");
            Assert.IsTrue(new System.IO.FileInfo(resultFile).Length > 500 * 1000, "mp4 file is not large enough");
        }
    }
}
