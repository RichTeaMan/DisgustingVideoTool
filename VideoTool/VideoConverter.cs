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
        public readonly static string[] VIDEO_EXTENSIONS = new[] { ".mp4", ".mkv", ".flv", ".avi", ".mov", ".m4v", ".mpg", ".wmv", ".webm" };

        public const string CONVERTED_VIDEO_PREFIX = "backup";

        private const string IN_PROGRESS_EXTENSION = ".convert.mp4";

        private const string FFMPEG_TEMPLATE = "-i {0} -c:v libx264 -b:v 8M -minrate 8M -preset medium -c:a aac -b:a 320K {1} -y -progress pipe:1";// -nostdin";

        private const string FFMPEG_FRAME_COUNT_TEMPLATE = "-progress pipe:1 -i {0} -map 0:v:0 -c copy -f null - ";

        private readonly static string exeDirectory = "./";
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
                Console.WriteLine($"ffmpeg saved to {programPath}.");
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

        public async Task<long?> FetchTotalVideoFrames(string videoPath)
        {
            Console.WriteLine("Fetching frame count...");
            long? frameCount = null;

            var command = string.Format(FFMPEG_FRAME_COUNT_TEMPLATE, videoPath);

            var processInfo = await FetchFfmpegProcess();
            processInfo.Arguments = command;

            using var process = new Process
            {
                StartInfo = processInfo
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e?.Data?.StartsWith("frame=") == true)
                {

                    string frameStr = e.Data.Replace("frame=", "");
                    if (long.TryParse(frameStr, out long frame))
                    {
                        frameCount = frame;
                    }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (frameCount.HasValue)
            {
                Console.WriteLine($"Video has {frameCount} frames.");
            }
            else
            {
                Console.WriteLine("Could not fetch frame count.");
            }

            return frameCount;
        }

        public async Task ConvertVideo(string videoPath)
        {
            var startTime = DateTimeOffset.Now;

            var totalFrameCount = await FetchTotalVideoFrames(videoPath);

            var fi = new FileInfo(videoPath);
            var outputVideo = videoPath.Replace(fi.Extension, ".mp4");
            var workingFile = videoPath.Replace(fi.Extension, IN_PROGRESS_EXTENSION);

            // ffmpeg -i sample_640x360.mkv -c:v libx264 -b:v 8M -minrate 8M -preset medium -c:a aac -b:a 320K sample_640x360_converted.mp4 -y -nostdin
            var command = string.Format(FFMPEG_TEMPLATE, videoPath, workingFile);

            var processInfo = await FetchFfmpegProcess();
            processInfo.Arguments = command;

            using var process = new Process
            {
                StartInfo = processInfo
            };

            long lastFrame = 0;
            process.OutputDataReceived += (sender, e) =>
            {
                if (e?.Data?.StartsWith("frame=") == true)
                {
                    string frameStr = e.Data.Replace("frame=", "");
                    if (long.TryParse(frameStr, out long frame))
                    {
                        lastFrame = frame;
                    }
                }
                else if (e?.Data?.StartsWith("fps=") == true)
                {
                    string fpsStr = e.Data.Replace("fps=", "");
                    if (double.TryParse(fpsStr, out double fps))
                    {
                        long? remainingFrames = null;
                        if (totalFrameCount.HasValue)
                        {
                            remainingFrames = totalFrameCount - lastFrame;
                        }
                        string progress;
                        if (remainingFrames.HasValue && fps > 0)
                        {
                            var eta = TimeSpan.FromSeconds(remainingFrames.Value / fps);
                            progress = $"{lastFrame}/{totalFrameCount} frame | FPS: {fps} | ETA: {eta}";
                        }
                        else
                        {
                            progress = $"{lastFrame}/??? frame | FPS: {fps} | ETA: ???";
                        }

                        if (Console.IsOutputRedirected)
                        {
                            Console.WriteLine(progress);
                        }
                        else
                        {
                            Console.Write(progress);
                            Console.SetCursorPosition(0, Console.CursorTop);
                        }
                    }
                }
                else if (e?.Data == "progress=end")
                {
                    Console.WriteLine();
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            var endTime = DateTimeOffset.Now;
            var conversionDuration = endTime - startTime;
            Console.WriteLine($"{fi.Name} converted in {conversionDuration}.");


            var backupPath = Path.Combine(fi.DirectoryName, CONVERTED_VIDEO_PREFIX + fi.Name);
            // change source file to back up name.
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            File.Move(videoPath, backupPath);
            if (File.Exists(outputVideo))
            {
                File.Delete(outputVideo);
            }
            File.Move(workingFile, outputVideo);
        }
    }
}
