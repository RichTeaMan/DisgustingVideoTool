using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class ImgurImage
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string type { get; set; }
        public bool animated { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int size { get; set; }
        public int views { get; set; }
        public long bandwidth { get; set; }
        public string vote { get; set; }
        public bool favorite { get; set; }
        public bool? nsfw { get; set; }
        public string section { get; set; }
        public string account_url { get; set; }
        public string account_id { get; set; }
        public string comment_preview { get; set; }
        public string link { get; set; }
        public string gifv { get; set; }
        public string webm { get; set; }
        public string mp4 { get; set; }
        public bool looping { get; set; }
    }
}
