using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete("ffmpeg.exe");
            File.Delete("ffmpeg");
            foreach (var file in Directory.EnumerateFiles(".", "backup*"))
            {
                string oldName = file.Replace("backup", "");
                if (File.Exists(oldName))
                {
                    File.Delete(file);
                }
                else
                {
                    File.Move(file, oldName);
                }
            }
            foreach (var file in Directory.EnumerateFiles(".", "temp*"))
            {
                File.Delete(file);
            }
            foreach (var file in Directory.EnumerateFiles(".", "*.mp4"))
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.AreEqual("2020-12-15-git-32586a42da-essentials_build-www.gyan.dev", version);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Assert.AreEqual("4.3.1-static", version);
            }
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
