using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

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

        public async Task FetchYoutube(string watch)
        {
            CreateSaveLocation();
            var url = GetYoutubeUrl(watch);
            await DownloadYoutubeVideo(url);
        }

        public async Task FetchYoutubePlaylist(string playlistToken)
        {
            var factory = new YoutubePlaylistFactory();
            var playlist = await factory.DownloadPlaylist(playlistToken);

            Console.WriteLine($"Downloading {playlist.Videos.Length} videos from playlist.");
            foreach (var videoUrl in playlist.Videos.Select(i => i.Id))
            {
                await FetchYoutube(videoUrl);
            }
        }

        private async Task DownloadYoutubeVideo(string url)
        {
            var youtube = new YoutubeClient();

            // You can specify video ID or URL
            var video = await youtube.Videos.GetAsync(url);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest
                .GetVideoOnly()
                .Where(s => s.Container == Container.Mp4)
                .WithHighestVideoQuality();
            if (streamInfo == null)
            {
                Console.WriteLine("No appropriate stream found for {0}.", url);
            }
            else
            {
                string fileName = Path.Combine(OutputDirectory, FileNameCleaner(video.Title + ".mp4"));
                if (File.Exists(fileName))
                {
                    Console.WriteLine("File with name {0} already exists. File will not be downloaded.", fileName);
                }
                else
                {
                    string fileNameDownload = fileName + ".download";
                    // Get the actual stream
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

                    // Download the stream to file
                    int lastProgress = -1;
                    IProgress<double> progress = new Progress<double>(progressValue =>
                    {
                        if (progressValue > lastProgress + 1)
                        {
                            lastProgress++;
                            Console.Write($"\rGetting '{video.Title}'. {lastProgress}% complete.");
                        }
                    });
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, fileNameDownload, progress);

                    File.Move(fileNameDownload, fileName);
                    Console.WriteLine($"\r'{video.Title}' downloaded to {fileName}");
                }
            }
        }

        private string FileNameCleaner(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
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
