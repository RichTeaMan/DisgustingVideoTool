using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoTool
{
    public class VideoConverter
    {
        private readonly static string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly static string programName = "ffmpeg.exe";
        private readonly string programPath = Path.Combine(exeDirectory, programName);

        private async Task DownloadFfmpeg()
        {
            string url = "https://github.com/GyanD/codexffmpeg/releases/download/2020-12-15-git-32586a42da/ffmpeg-2020-12-15-git-32586a42da-essentials_build.zip";

            string ffmpegZipLocation = Path.Combine(exeDirectory, "ffmpeg.zip");
            string ffmpegUnzipLocation = Path.Combine(exeDirectory, "ffmpeg");

            try
            {
                using var webclient = new WebClient();
                await webclient.DownloadFileTaskAsync(url, ffmpegZipLocation);

                ZipFile.ExtractToDirectory(ffmpegZipLocation, ffmpegUnzipLocation);

                var programPathFromArchive = Directory.EnumerateFiles(ffmpegUnzipLocation, programName, SearchOption.AllDirectories).FirstOrDefault();
                if (programPathFromArchive == null)
                {
                    throw new Exception("Could not find ffmpeg.exe in zip archive.");
                }
                File.Move(programPathFromArchive, programPath);
            }
            finally
            {
                try
                {
                    Directory.Delete(ffmpegUnzipLocation, true);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
                try
                {
                    File.Delete(ffmpegZipLocation);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async Task<ProcessStartInfo> FetchFfmpegProcess()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new Exception("Video conversion only supported on Windows.");
            }

            if (!File.Exists(programName))
            {
                await DownloadFfmpeg();
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = programPath,
                ///Arguments = command,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            return startInfo;
        }

        public async Task<string> FetchFfmpegVersion()
        {
            var processInfo = await FetchFfmpegProcess();
            processInfo.Arguments = "-version";

            using var process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Regex rx = new Regex(@"(?<=ffmpeg version )[^\s]+");
            return rx.Match(output).Value;
        }
    }
}
