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
        private const string CONVERTED_VIDEO_PREFIX = "backup";
        private const string IN_PROGRESS_EXTENSION = ".convert.mp4";

        private const string FFMPEG_TEMPLATE = "-i {0} -c:v libx264 -b:v 8M -minrate 8M -preset medium -c:a aac -b:a 320K {1} -y -nostdin";

        private readonly static string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly static string programName = "ffmpeg.exe";
        private readonly static string programPath = Path.Combine(exeDirectory, programName);

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

        public async Task ConvertVideo(string videoPath)
        {
            var fi = new FileInfo(videoPath);
            var outputVideo = videoPath.Replace(fi.Extension, ".mp4");
            var workingFile = videoPath.Replace(fi.Extension, IN_PROGRESS_EXTENSION);

            // ffmpeg -i sample_640x360.mkv -c:v libx264 -b:v 8M -minrate 8M -preset medium -c:a aac -b:a 320K sample_640x360_converted.mp4 -y -nostdin
            var command = string.Format(FFMPEG_TEMPLATE, videoPath, workingFile);

            Console.WriteLine(command);

            var processInfo = await FetchFfmpegProcess();
            processInfo.Arguments = command;

            using var process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            System.Console.WriteLine("Program output:");
            System.Console.WriteLine(output);

            var newPath = Path.Combine(fi.DirectoryName, CONVERTED_VIDEO_PREFIX + fi.Name);
            // change source file to back up name.
            File.Move(videoPath, newPath);
            File.Move(workingFile, outputVideo);
        }
    }
}
