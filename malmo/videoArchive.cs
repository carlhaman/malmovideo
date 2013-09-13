using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace malmo
{
    public class videoArchive
    {
        public List<videoCategory> categories { get; set; }
    }
    public class videoCategory
    {
        public string name { get; set; }
        public List<videoItem> videos { get; set; }
    }
    public class videoItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public string videoStillURL { get; set; }
        public string thumbnailURL { get; set; }
        public string length { get; set; }
        public string playsTotal { get; set; }
    }
}