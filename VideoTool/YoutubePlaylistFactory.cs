using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class YoutubePlaylistFactory
    {
        private string urlTemplate = "https://www.googleapis.com/youtube/v3/playlistItems?part=id,contentDetails&playlistId={0}&key={1}";

        public YoutubePlaylist DownloadPlaylist(string playlistId, string nextPageToken = null)
        {
            var url = string.Format(urlTemplate, playlistId, Keys.YoutubeKey);

            if (nextPageToken != null)
            {
                url += "&pageToken=" + nextPageToken;
            }
            
            using (var client = new WebClient())
            {
                var payload = client.DownloadString(url);
                var playlist =  ConvertPlaylist(payload);
                if(!string.IsNullOrEmpty(playlist.nextPageToken))
                {
                    var nextPlaylist = DownloadPlaylist(playlistId, playlist.nextPageToken);
                    playlist.AddItems(nextPlaylist.items);
                }
                return playlist;
            }
        }

        public YoutubePlaylist ConvertPlaylist(string payload)
        {
            return JsonConvert.DeserializeObject<YoutubePlaylist>(payload);
        }
    }
}
