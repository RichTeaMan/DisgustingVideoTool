using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class ImgurResponse<T>
    {
        public T data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
    }
}
