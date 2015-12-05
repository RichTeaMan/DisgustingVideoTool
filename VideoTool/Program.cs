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

        readonly static string[] VIDEO_EXTENSIONS = new[] { ".mp4", ".mkv", ".flv", ".avi", ".mov", ".m4v", ".mpg", ".wmv", ".webm" };

        readonly static string IN_PROGRESS_EXTENSION = ".convert.mp4";

        const string HANDBRAKE_TEMPLATE = "-i \"{0}\" -o \"{1}\"  -f mp4 -e x264";

        const string CONVERTED_VIDEO_PREFIX = "backup";

        static void Main(string[] args)
        {
            MethodInvoker command = null;
            try
            {
                command = ClCommandAttribute.GetCommand(typeof(Program), args);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing command:");
                Console.WriteLine(ex.Message);
            }
            if (command != null)
            {
                try
                {
                    command.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running command:");
                    Console.WriteLine(ex.Message);

                    var inner = ex.InnerException;
                    while (inner != null)
                    {
                        Console.WriteLine(inner);
                        Console.WriteLine();
                        inner = inner.InnerException;
                    }

                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        [ClCommand("imgur")]
        public static void FetchImgurAlbum(
            [ClArgs("album", "a")]
            string[] albums
            )
        {
            var pictureDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var imgurAlbumDirectory = new ImgurAlbumFactory();
            using (var webClient = new WebClient())
                foreach (var album in albums)
                {
                    var imgurAlbum = imgurAlbumDirectory.DownloadAlbum(album);
                    var albumDir = Path.Combine(pictureDir, "imgur", imgurAlbum.id);
                    Directory.CreateDirectory(albumDir);
                    Console.WriteLine("Saving {0} images from {1} to {2}.", imgurAlbum.images_count, album, albumDir);
                    int count = 1;
                    foreach (var imgurImage in imgurAlbum.images)
                    {
                        var imageFilename = new Uri(imgurImage.link).Segments.Last();
                        var imagePath = Path.Combine(albumDir, imageFilename);
                        webClient.DownloadFile(imgurImage.link, imagePath);
                        Console.Write("\rDownloaded {0} of {1}.", count, imgurAlbum.images_count);
                        count++;
                    }
                    Console.WriteLine();
                    Console.WriteLine("{0} complete.", album);
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

        [ClCommand("ytpl")]
        public static void FetchYoutubePlaylist(
            [ClArgs("playlist", "pl")]
            string playlistToken,
            [ClArgs("outputDirectory", "dir")]
            string outputDirectory = null
            )
        {
            var factory = new YoutubePlaylistFactory();
            var playlist = factory.DownloadPlaylist(playlistToken);

            Console.WriteLine("Downloading {0} videos from playlist.", playlist.items.Length);
            FetchYoutube(playlist.items.Select(i => i.contentDetails.videoId).ToArray());

            Console.WriteLine("Playlist download complete.");
        }

        [ClCommand("convert")]
        public static void Convert(
            [ClArgs("mp4")]
            bool mp4 = false
            )
        {
            // get videos to convert
            var videoFiles = GetVideoFiles();

            foreach (var videoFile in videoFiles)
            {
                // skip mp4 files if flag is not enabled
                if (videoFile.Extension == ".mp4" && !mp4)
                {
                    continue;
                }
                try
                {
                    ConvertVideo(videoFile.FullName);
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
            var workingFile = videoPath.Replace(fi.Extension, IN_PROGRESS_EXTENSION);

            var command = string.Format(HANDBRAKE_TEMPLATE, videoPath, workingFile);

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
            // change source file to back up name.
            File.Move(videoPath, newPath);
            File.Move(workingFile, output);
        }

        [ClCommand("list-backups")]
        public static void ListBackups()
        {
            var list = GetBackupVideoFiles().Select(fi => fi.Name).ToArray();
            if (list.Count() == 0)
            {
                Console.WriteLine("No backup video files.");
            }
            else
            {
                Console.WriteLine("{0} backup video files:", list.Count());
                foreach (var f in list)
                {
                    Console.WriteLine(f);
                }
                Console.WriteLine();
                Console.WriteLine("Run delete-backups to delete.");
            }
        }

        [ClCommand("restore-backups")]
        public static void RestoreBackups()
        {
            var list = GetBackupVideoFiles();
            foreach (var f in list)
            {
                var newName = f.Name.Remove(0, CONVERTED_VIDEO_PREFIX.Length);
                var newFullName = Path.Combine(f.DirectoryName, newName);
                Console.WriteLine("Restoring {0} to {1}.", f.FullName, newFullName);
                File.Move(f.FullName, newFullName);
            }
        }

        [ClCommand("delete-backups")]
        public static void DeleteBackups()
        {
            var list = GetBackupVideoFiles().ToArray();
            if (list.Count() == 0)
            {
                Console.WriteLine("No backup video files.");
            }
            else
            {
                Console.WriteLine("Deleting {0} backup video files:", list.Count());
                int deleted = 0;
                int failed = 0;
                foreach (var f in list)
                {
                    try
                    {
                        Console.WriteLine("Deleting {0}.", f.Name);
                        File.Delete(f.FullName);
                        deleted++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not delete {0}: {1}", f.Name, ex.Message);
                        failed++;
                    }
                }
                Console.WriteLine();
                Console.WriteLine("{0} backups deleted. {1} failed.", deleted, failed);
            }
        }

        [ClCommand("rename")]
        public static void RenameFiles(
            [ClArgs("pattern")]
            string pattern,
            [ClArgs("replace")]
            string replace = null,
            [ClArgs("all")]
            bool all = false)
        {
            if (replace == null)
                replace = string.Empty;

            IEnumerable<FileInfo> renameFiles;
            if (all)
            {
                var curDir = Directory.GetCurrentDirectory();
                renameFiles = Directory.EnumerateFiles(curDir).Select(p => new FileInfo(p));
            }
            else
            {
                renameFiles = GetVideoFiles();
            }
            foreach (var f in renameFiles)
            {
                // get name without extension so extension is not modified.
                var fileName = f.Name.Replace(f.Extension, string.Empty);
                var newName = fileName.Replace(pattern, replace) + f.Extension;
                var newFullName = Path.Combine(f.DirectoryName, newName);
                Console.WriteLine("Renaming {0} to {1}.", f.FullName, newFullName);
                File.Move(f.FullName, newFullName);
            }
            Console.WriteLine("Files renamed.");
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

        private static IEnumerable<FileInfo> GetConvertableVideoFiles()
        {
            var curDir = Directory.GetCurrentDirectory();
            foreach (var f in Directory.EnumerateFiles(curDir))
            {
                var fi = new FileInfo(f);
                if (VIDEO_EXTENSIONS.Contains(fi.Extension.ToLower()))
                {
                    yield return fi;
                }
            }
        }

        private static IEnumerable<FileInfo> GetVideoFiles()
        {
            var backups = new HashSet<string>(GetBackupVideoFiles().Select(v => v.FullName));

            return GetConvertableVideoFiles().Where(fi => !fi.Name.StartsWith(CONVERTED_VIDEO_PREFIX) && !backups.Contains(fi.FullName));
        }

        private static IEnumerable<FileInfo> GetBackupVideoFiles()
        {
            return GetConvertableVideoFiles().Where(fi => fi.Name.StartsWith(CONVERTED_VIDEO_PREFIX));
        }

        private static void DownloadYoutubeVideo(string saveLocation, string url)
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
                string fileName = Path.Combine(saveLocation, (info.Title + ".mp4"));
                if (File.Exists(fileName))
                {
                    Console.WriteLine("File with name {0} already exists. File will not be downloaded.", fileName);
                }
                else
                {
                    string fileNameDownload = fileName + ".download";
                    var downloader = new VideoDownloader(info, fileNameDownload);

                    downloader.DownloadProgressChanged += downloader_DownloadProgressChanged;
                    downloader.Execute();
                    lastProgress = -1;

                    File.Move(fileNameDownload, fileName);
                    Console.WriteLine("\r'{0}' downloaded to {1}", downloader.Video.Title, fileName);
                }
            }
        }

        private static IEnumerable<string> GetYoutubeUrls(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                if (!url.ToLower().Contains("youtube"))
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
            if (string.IsNullOrEmpty(outputDirectory))
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

        private static void downloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
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
