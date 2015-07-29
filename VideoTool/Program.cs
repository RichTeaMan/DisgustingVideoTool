using CommandLineParser;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace VideoTool
{
    class Program
    {
        const string YOUTUBE_TEMPLATE = "http://www.youtube.com/watch?v={0}";

        const string SAVE_FOLDER = "YoutubeVideos";

        readonly static string[] VIDEO_EXTENSIONS = new[] { ".mkv", ".flv", ".avi", ".mov" };

        const string HANDBRAKE_TEMPLATE = "-i \"{0}\" -o \"{1}\"";

        const string CONVERTED_VIDEO_PREFIX = "backup";

        static void Main(string[] args)
        {
            try
            {
                var command = ClCommandAttribute.GetCommand(typeof(Program), args);
                command.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing command:");
                Console.WriteLine(ex.Message);
            }
        }

        [ClCommand("yt")]
        public static void FetchYoutube(
            [ClArgs("watch", "w")]
            string[] watchs,
            [ClArgs("outputDirectory", "dir")]
            string outputDirectory = null
            )
        {
            string saveLocation = GetSaveLocation(outputDirectory);

            var urls = GetYoutubeUrls(watchs).ToArray();

            foreach (var url in urls)
            {
                try
                {
                    DownloadYoutubeVideo(saveLocation, url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured: {0}", ex.Message);
                }

            }
            Console.WriteLine("Downloads complete!");
        }

        [ClCommand("convert")]
        public static void Convert()
        {
            // get videos to convert
            var videoPaths = GetVideoFiles();

            foreach (var videoPath in videoPaths)
            {
                try
                {
                    ConvertVideo(videoPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured: {0}", ex.Message);
                }

            }
            Console.WriteLine("Conversion complete!");
        }

        private static void ConvertVideo(string videoPath)
        {
            var fi = new FileInfo(videoPath);
            var output = videoPath.Replace(fi.Extension, ".mp4");

            var command = string.Format(HANDBRAKE_TEMPLATE, videoPath, output);

            var startInfo = new ProcessStartInfo()
            {
                FileName = "handbrakecli.exe",
                Arguments = command,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,                
            };

            using (var process = new Process())
            {
                process.OutputDataReceived += Process_OutputDataReceived;
                process.StartInfo = startInfo;
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }

            var newPath = Path.Combine(fi.DirectoryName, CONVERTED_VIDEO_PREFIX + fi.Name);

            File.Move(videoPath, newPath);
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine();
            }
            else
            {
                Console.Write(e.Data);
                if (!e.Data.EndsWith(Environment.NewLine))
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
            }
        }

        private static IEnumerable<string> GetVideoFiles()
        {
            var curDir = Directory.GetCurrentDirectory();
            foreach (var f in Directory.EnumerateFiles(curDir))
            {
                var fi = new FileInfo(f);
                if (!fi.Name.StartsWith(CONVERTED_VIDEO_PREFIX) && VIDEO_EXTENSIONS.Contains(fi.Extension.ToLower()))
                {
                    yield return f;
                }
            }
        }

        private static void DownloadYoutubeVideo(string saveLocation, string url)
        {
            var infos = DownloadUrlResolver.GetDownloadUrls(url).ToArray();
            var info = infos.Where(i => i.VideoType == VideoType.Mp4).OrderByDescending(i => i.Resolution).FirstOrDefault();
            if (info == null)
            {
                Console.WriteLine("No appropriate stream found for {0}.", url);
            }
            else
            {
                string fileName = Path.Combine(saveLocation, (info.Title + ".mp4"));
                var downloader = new VideoDownloader(info, fileName);

                downloader.DownloadProgressChanged += downloader_DownloadProgressChanged;
                downloader.Execute();
                lastProgress = -1;
                Console.WriteLine("\r'{0}' downloaded to {1}", downloader.Video.Title, fileName);
            }
        }

        static IEnumerable<string> GetYoutubeUrls(IEnumerable<string> urls)
        {
            foreach(var url in urls)
            {
                if(!url.ToLower().Contains("youtube"))
                {
                    var urlStr = string.Format(YOUTUBE_TEMPLATE, url);
                    yield return urlStr;
                }
                else
                {
                    yield return url;
                }
            }
        }

        private static string GetSaveLocation(string outputDirectory)
        {
            if(string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SAVE_FOLDER);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
            }
            return outputDirectory;
        }

        static int lastProgress = -1;

        static void downloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            if (e.ProgressPercentage > lastProgress + 1)
            {
                var downloader = (VideoDownloader)sender;
                lastProgress++;
                
                Console.Write("\rGetting '{0}'. {1}% complete.", downloader.Video.Title, lastProgress);
                
            }
        }
    }
}
