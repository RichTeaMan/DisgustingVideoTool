using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;

namespace VideoTool
{
    public class YoutubePlaylistFactory
    {
        public async Task<YoutubePlaylist> DownloadPlaylist(string playlistId, string nextPageToken = null)
        {
            var resultPlaylist = new YoutubePlaylist();
            var youtube = new YoutubeClient();

            // Get playlist metadata
            var playlist = await youtube.Playlists.GetAsync(playlistId);
            resultPlaylist.Author = playlist.Author;
            resultPlaylist.Title = playlist.Title;
            resultPlaylist.Description = playlist.Description;

            var playlistVideos = await youtube.Playlists.GetVideosAsync(playlist.Id);
            resultPlaylist.AddItems(playlistVideos);
            return resultPlaylist;
        }

        public YoutubePlaylist ConvertPlaylist(string payload)
        {
            return JsonConvert.DeserializeObject<YoutubePlaylist>(payload);
        }
    }
}
