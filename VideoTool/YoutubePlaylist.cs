using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace VideoTool
{
    public class YoutubePlaylist
    {
        public string Author { get; set; }
        
        public string Title { get; set; }

        public string Description { get; set; }

        public Video[] Videos { get; private set; } = new Video[0];

        public YoutubePlaylist AddItems(IEnumerable<Video> addedItems)
        {
            var newItems = new List<Video>();
            newItems.AddRange(Videos);
            newItems.AddRange(addedItems);
            Videos = newItems.ToArray();
            return this;
        }

    }
}
