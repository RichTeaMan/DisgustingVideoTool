using RichTea.CommandLineParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class Program
    {
        public static string VersionNumber => typeof(Program).Assembly
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
          .InformationalVersion;

        static void Main(string[] args)
        {
            Console.WriteLine("Disgusting Video Converter");
            Console.WriteLine($"Thomas Holmes 2015-2020. {VersionNumber}");

            Parser.ParseCommand<Program>(args);
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
        public async static Task FetchYoutube(
            [ClArgs("watch", "w")]
            string[] watchs = null,
            [ClArgs("playlist", "pl")]
            string[] playlists = null,
            [ClArgs("outputDirectory", "dir")]
            string outputDirectory = null
            )
        {
            var youtubeDownloader = new YoutubeDownloader();
            if (outputDirectory != null)
            { youtubeDownloader.OutputDirectory = outputDirectory; }

            if (watchs != null)
            {
                foreach (var watch in watchs)
                {
                    await youtubeDownloader.FetchYoutube(watch);
                }
            }
            if (playlists != null)
            {
                foreach (var playlist in playlists)
                {
                    await youtubeDownloader.FetchYoutubePlaylist(playlist);
                }
            }
            Console.WriteLine("Downloads complete!");
        }

        [ClCommand("convert")]
        public async static Task Convert(
            [ClArgs("file", "f")]
            string filename = null,
            [ClArgs("mp4")]
            bool mp4 = false,
            [ClArgs("start", "s")]
            string start = null,
            [ClArgs("duration", "d")]
            string duration = null
            )
        {
            if (string.IsNullOrEmpty(filename))
            {
                if (!string.IsNullOrEmpty(start) || !string.IsNullOrEmpty(duration))
                {
                    Console.WriteLine("Start or duration parameters must be used with a file parameter.");
                    return;
                }

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
                        var converter = new VideoConverter();
                        await converter.ConvertVideo(videoFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occured: {0}", ex);
                    }

                }
                Console.WriteLine("Conversion complete!");
            }
            else
            {
                TimeSpan? startTimeSpan = null;
                TimeSpan? durationTimeSpan = null;

                if (!string.IsNullOrEmpty(start))
                {
                    if (int.TryParse(start, out int startSeconds))
                    {
                        startTimeSpan = new TimeSpan(0, 0, startSeconds);
                    }
                    else if (TimeSpan.TryParse(start, out TimeSpan _startTimeSpan))
                    {
                        startTimeSpan = _startTimeSpan;
                    }
                    else
                    {
                        Console.WriteLine($"Cannot parse start time '{start}'.");
                    }
                }
                if (!string.IsNullOrEmpty(duration))
                {
                    if (int.TryParse(duration, out int durationSeconds))
                    {
                        durationTimeSpan = new TimeSpan(0, 0, durationSeconds);
                    }
                    else if (TimeSpan.TryParse(duration, out TimeSpan _durationTimeSpan))
                    {
                        durationTimeSpan = _durationTimeSpan;
                    }
                    else
                    {
                        Console.WriteLine($"Cannot parse duration time '{duration}'.");
                    }
                }

                try
                {
                    var converter = new VideoConverter();
                    await converter.ConvertVideo(filename, startTimeSpan, durationTimeSpan);
                    Console.WriteLine("Conversion complete!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured: {0}", ex);
                }
            }
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
                var newName = f.Name.Remove(0, VideoConverter.CONVERTED_VIDEO_PREFIX.Length);
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
                        Console.WriteLine("Could not delete {0}: {1}", f.Name, ex);
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
                if (VideoConverter.VIDEO_EXTENSIONS.Contains(fi.Extension.ToLower()))
                {
                    yield return fi;
                }
            }
        }

        private static IEnumerable<FileInfo> GetVideoFiles()
        {
            var backups = new HashSet<string>(GetBackupVideoFiles().Select(v => v.FullName));

            return GetConvertableVideoFiles().Where(fi => !fi.Name.StartsWith(VideoConverter.CONVERTED_VIDEO_PREFIX) && !backups.Contains(fi.FullName));
        }

        private static IEnumerable<FileInfo> GetBackupVideoFiles()
        {
            return GetConvertableVideoFiles().Where(fi => fi.Name.StartsWith(VideoConverter.CONVERTED_VIDEO_PREFIX));
        }

    }
}
