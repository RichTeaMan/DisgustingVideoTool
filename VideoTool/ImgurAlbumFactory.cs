using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class ImgurAlbumFactory
    {
        private const string AlbumUrl = "https://api.imgur.com/3/album/{0}";
        private const string AuthHeaderName = "Authorization";
        private const string AuthHeaderValue = "Client-ID {0}";

        public ImgurAlbum DownloadAlbum(string token)
        {
            using (var client = CreateWebClient())
            {
                var url = string.Format(AlbumUrl, token);
                var response = client.DownloadString(url);
                var album = ConvertPlaylist(response);
                return album;
            }
        }

        private WebClient CreateWebClient()
        {
            var webClient = new WebClient();
            string headerValue = string.Format(AuthHeaderValue, Keys.ImgurId);
            webClient.Headers.Add(AuthHeaderName, headerValue);
            return webClient;
        }

        public ImgurAlbum ConvertPlaylist(string payload)
        {
            var imgurResponse = JsonConvert.DeserializeObject<ImgurResponse<ImgurAlbum>>(payload);
            return imgurResponse.data;
        }
    }
}
