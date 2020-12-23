using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

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
    }
}
