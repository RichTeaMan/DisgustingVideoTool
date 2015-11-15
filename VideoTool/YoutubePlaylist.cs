using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class YoutubePlaylist
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public Pageinfo pageInfo { get; set; }
        public Item[] items { get; set; }

        public YoutubePlaylist AddItems(IEnumerable<Item> addedItems)
        {
            var newItems = new List<Item>();
            newItems.AddRange(items);
            newItems.AddRange(addedItems);
            items = newItems.ToArray();
            return this;
        }

        public class Pageinfo
        {
            public int totalResults { get; set; }
            public int resultsPerPage { get; set; }
        }

        public class Item
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public string id { get; set; }
            public Contentdetails contentDetails { get; set; }
        }

        public class Contentdetails
        {
            public string videoId { get; set; }
        }

    }
}
