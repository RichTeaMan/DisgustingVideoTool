using CommandLineParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace YouTubeFetcher
{
    class Program
    {
        const string YOUTUBE_TEMPLATE = "http://www.youtube.com/watch?v={0}";

        const string SAVE_FOLDER = "YoutubeVideos";

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
