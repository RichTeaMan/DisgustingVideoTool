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
        private int sampleFileCount = 0;

        private string rootVideoFileName = "sample";

        private VideoConverter videoConverter;

        [TestInitialize]
        public void Initialise()
        {
            Cleanup();
            videoConverter = new VideoConverter();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            foreach(var file in Directory.EnumerateFiles(".", "temp-*"))
            {
                File.Delete(file);
            }
            foreach (var file in Directory.EnumerateFiles(".", "backup*"))
            {
                File.Delete(file);
            }
        }

        private string CopySampleVideoFile()
        {
            string input = $"temp-{sampleFileCount}-{rootVideoFileName}.mkv";
            File.Copy(rootVideoFileName + ".mkv", input);
            sampleFileCount++;
            return input;
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
            string input = CopySampleVideoFile();
            string resultFile = input.Replace(".mkv", ".mp4");

            await videoConverter.ConvertVideo(input);

            Assert.IsTrue(File.Exists(resultFile), "mp4 file does not exist.");
            Assert.IsTrue(new System.IO.FileInfo(resultFile).Length > 500 * 1000, "mp4 file is not large enough");
        }

        [TestMethod]
        public async Task FetchTotalVideoFramesTest()
        {
            string input = CopySampleVideoFile();

            var frames = await videoConverter.FetchTotalVideoFrames(input);

            Assert.AreEqual(336L, frames);
        }
    }
}
