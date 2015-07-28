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
            if (args.Count() == 0)
            {
                Console.WriteLine("Supply a list of v parameters from Youtube videos to download them.");
                return;
            }
            string saveLocation;
            var saveLocationArg = args.FirstOrDefault(a => a.StartsWith("-save:"));
            if (saveLocationArg != null)
                saveLocation = saveLocationArg.Substring(saveLocationArg.IndexOf(':'));
            else
            {
                saveLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SAVE_FOLDER);
                if (!Directory.Exists(saveLocation))
                    Directory.CreateDirectory(saveLocation);
            }

            var urls = args.Where(a => !a.Contains(':')).Select(u => string.Format(YOUTUBE_TEMPLATE, u));
            foreach (var url in urls)
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured: {0}", ex.Message);
                }

            }
            Console.WriteLine("Downloads complete!");
        }

        static int lastProgress = -1;

        static void downloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            if (e.ProgressPercentage > lastProgress + 1)
            {
                var downloader = sender as VideoDownloader;
                lastProgress++;
                
                Console.Write("\rGetting '{0}'. {1}% complete.", downloader.Video.Title, lastProgress);
                
            }
        }
    }
}
