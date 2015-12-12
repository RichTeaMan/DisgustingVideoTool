using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace VideoTool
{
    public class YoutubeDownloader
    {
        const string YOUTUBE_TEMPLATE = "http://www.youtube.com/watch?v={0}";
        const string SAVE_FOLDER = "YoutubeVideos";

        public string OutputDirectory { get; set; }

        public YoutubeDownloader()
        {
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SAVE_FOLDER);
        }

        public void FetchYoutube(string watch)
        {
            CreateSaveLocation();
            var url = GetYoutubeUrl(watch);
            DownloadYoutubeVideo(url);
        }

        public void FetchYoutubePlaylist(string playlistToken)
        {
            var factory = new YoutubePlaylistFactory();
            var playlist = factory.DownloadPlaylist(playlistToken);

            Console.WriteLine("Downloading {0} videos from playlist.", playlist.items.Length);
            foreach (var videoUrl in playlist.items.Select(i => i.contentDetails.videoId))
            {
                FetchYoutube(videoUrl);
            }
        }

        private void DownloadYoutubeVideo(string url)
        {
            var infos = DownloadUrlResolver.GetDownloadUrls(url).ToArray();
            var info = infos
                .Where(i => i.VideoType == VideoType.Mp4)
                .OrderByDescending(i => i.Resolution)
                .FirstOrDefault();
            if (info == null)
            {
                Console.WriteLine("No appropriate stream found for {0}.", url);
            }
            else
            {
                string fileName = Path.Combine(OutputDirectory, fileNameCleaner(info.Title + ".mp4"));
                if (File.Exists(fileName))
                {
                    Console.WriteLine("File with name {0} already exists. File will not be downloaded.", fileName);
                }
                else
                {
                    string fileNameDownload = fileName + ".download";
                    var downloader = new VideoDownloader(info, fileNameDownload);

                    int lastProgress = -1;
                    downloader.DownloadProgressChanged += (sender, eventArgs) =>
                    {
                        if (eventArgs.ProgressPercentage > lastProgress + 1)
                        {
                            lastProgress++;
                            Console.Write("\rGetting '{0}'. {1}% complete.", downloader.Video.Title, lastProgress);
                        }
                    };
                    downloader.DownloadFinished += (sender, eventArgs) =>
                    {
                        if (downloader.BytesToDownload == null)
                        {
                            File.Move(fileNameDownload, fileName);
                            Console.WriteLine("\r'{0}' downloaded to {1}", downloader.Video.Title, fileName);
                        }
                        else
                        {
                            Console.WriteLine("Failed to download {0}.", downloader.Video.Title);
                        }
                    };
                    downloader.Execute();
                }
            }
        }

        private string fileNameCleaner(string fileName)
        {
            foreach(var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), string.Empty);
            }
            return fileName;
        }

        private string GetYoutubeUrl(string watch)
        {
            var url = watch;
            if (!watch.ToLower().Contains("youtube"))
            {
                url = string.Format(YOUTUBE_TEMPLATE, url);
            }
            return url;
        }

        private void CreateSaveLocation()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }
    }
}
