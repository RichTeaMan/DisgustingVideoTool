using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace VideoTool.Test
{
    [TestClass]
    public class IntegrationTests
    {
        private Process VideoToolProcess()
        {
            Process externalProcess = new Process();
            externalProcess.StartInfo.FileName = "VideoTool";
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
            using var process = VideoToolProcess();

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsNotNull(output);
        }

        [TestMethod]
        public void ConvertTest()
        {
            using var process = VideoToolProcess();

            process.StartInfo.Arguments = "convert";
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Videotool convert output:");
            Console.WriteLine(output);
            Console.WriteLine("-----------");
            Console.WriteLine();

            Assert.IsTrue(File.Exists("sample.mp4"), "sample.mp4 does not exist.");
        }
    }
}
