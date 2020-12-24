using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VideoTool.Test
{
    [TestClass]
    public class IntegrationTests
    {
        private Process VideoToolProcess(string argument)
        {
            Process externalProcess = new Process();
            externalProcess.StartInfo.FileName = "dotnet";
            externalProcess.StartInfo.Arguments = $"VideoTool.dll {argument}";
            externalProcess.StartInfo.RedirectStandardOutput = true;
            externalProcess.StartInfo.UseShellExecute = false;
            return externalProcess;
        }

        [TestInitialize]
        public void Initialise()
        {
            Cleanup();
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
            foreach (var file in Directory.EnumerateFiles(".", "*.mp4"))
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public void NoArgsTest()
        {
            try
            {
                using var process = VideoToolProcess(string.Empty);

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine("Videotool output:");
                Console.WriteLine(output);
                Console.WriteLine("-----------");
                Console.WriteLine();

                Assert.IsNotNull(output);
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Files:");
                foreach(var f in Directory.EnumerateFiles(".", "*", SearchOption.AllDirectories))
                {
                    Console.WriteLine(f);
                }
                Assert.Fail("Should not throw.");
            }
        }

        [TestMethod]
        public void ConvertTest()
        {
            using var process = VideoToolProcess("convert");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");
        }

        [TestMethod]
        public void ConvertBeginningPortionNoFileTest()
        {
            using var process = VideoToolProcess("convert -s 2");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsFalse(File.Exists("sample.mp4"), "sample.mp4 does not exist.");
            Assert.IsTrue(output?.Contains("Start or duration parameters must be used with a file parameter.") == true, "Start and duration parameters not allowed without file.");
        }

        [TestMethod]
        public void ConvertDurationPortionNoFileTest()
        {
            using var process = VideoToolProcess("convert -d 2");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsFalse(File.Exists("sample.mp4"), "sample.mp4 does not exist.");
            Assert.IsTrue(output?.Contains("Start or duration parameters must be used with a file parameter.") == true, "Start and duration parameters not allowed without file.");
        }

        [TestMethod]
        public async Task ConvertBeginningPortionIntegerTest()
        {
            using var process = VideoToolProcess("convert -f sample.mkv -s 2");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");

            var videoConverter = new VideoConverter();
            var frameCount = await videoConverter.FetchTotalVideoFrames("sample.mp4");
            Assert.AreEqual(12L * 24, frameCount);
        }

        [TestMethod]
        public async Task ConvertBeginningPortionTimeSpanTest()
        {
            using var process = VideoToolProcess("convert -f sample.mkv -s 00:00:02");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");

            var videoConverter = new VideoConverter();
            var frameCount = await videoConverter.FetchTotalVideoFrames("sample.mp4");
            Assert.AreEqual(12L * 24, frameCount);
        }

        [TestMethod]
        public async Task ConvertDurationPortionIntegerTest()
        {
            using var process = VideoToolProcess("convert -f sample.mkv -d 5");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");

            var videoConverter = new VideoConverter();
            var frameCount = await videoConverter.FetchTotalVideoFrames("sample.mp4");
            Assert.AreEqual(5L * 24, frameCount);
        }

        [TestMethod]
        public async Task ConvertDurationPortionTimeSpanTest()
        {
            using var process = VideoToolProcess("convert -f sample.mkv -d 00:00:05");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");

            var videoConverter = new VideoConverter();
            var frameCount = await videoConverter.FetchTotalVideoFrames("sample.mp4");
            Assert.AreEqual(5L * 24, frameCount);
        }

        [TestMethod]
        public async Task ConvertBeginningDurationPortionTimeSpanTest()
        {
            using var process = VideoToolProcess("convert -f sample.mkv -s 00:00:02 -d 00:00:05");
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");

            var videoConverter = new VideoConverter();
            var frameCount = await videoConverter.FetchTotalVideoFrames("sample.mp4");
            Assert.AreEqual(5L * 24, frameCount);
        }
    }
}
