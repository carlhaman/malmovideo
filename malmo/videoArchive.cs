using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;

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
        public List<string> tags { get; set; }
    }
    public class buildVideoArchive
    {

        public videoArchive render(bool komin)
        {
            return build(komin);
        }

        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        private static string MReadToken = System.Configuration.ConfigurationManager.AppSettings["M_READ_URL_TOKEN"].ToString();

        private videoArchive build(bool komin)
        {
            videoArchive archive = new videoArchive();
            archive.categories = new List<videoCategory>();

            string kfPlaylistBcId = "2623641282001";
            string mArchivePlayerBcId = "1180742924001";
            if (komin) { mArchivePlayerBcId = "1213665896001"; }
            string videoFields = "id,name,shortDescription,videoStillURL,thumbnailURL,length,playsTotal,tags,customFields";

            var mRequest = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlists_for_player_id&player_id={0}&video_fields={1}&token={2}", mArchivePlayerBcId, videoFields, MReadToken));
            var kfRequest = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlist_by_id&playlist_id={0}&video_fields={1}&token={2}", kfPlaylistBcId, videoFields, KFReadToken));
            mRequest.Method = "POST";
            kfRequest.Method = "POST";

            //Get Malmö Account Items
            try
            {
                var response = mRequest.GetResponse();
                Stream dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();

                    if (BCResponseString != null && BCResponseString != "null")
                    {
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);

                        foreach (dynamic category in results.items)
                        {
                            videoCategory cat = new videoCategory();
                            cat.name = category.name;
                            cat.videos = new List<videoItem>();
                            foreach (dynamic video in category.videos)
                            {
                                bool kominVideo = false;
                                if (video.customFields != null)
                                {
                                    var customFields = video.customFields;
                                    foreach (dynamic field in customFields)
                                    {
                                        if (field.Name != null)
                                        {
                                            if (field.Name == "targetgroup")
                                            {
                                                if (field.Value == "Komin") { kominVideo = true; }
                                            }

                                        }
                                    }
                                }
                                if (!kominVideo || kominVideo && komin)
                                {
                                    videoItem item = new videoItem();
                                    item.id = video.id;
                                    item.name = video.name.ToString().Replace("\"", "&quot");
                                    item.length = video.length;
                                    item.playsTotal = video.playsTotal;
                                    item.thumbnailURL = video.thumbnailURL;
                                    item.videoStillURL = video.videoStillURL;
                                    item.shortDescription = video.shortDescription.ToString().Replace("\"", "&quot");
                                    if (video.tags != null)
                                    {
                                        item.tags = new List<string>();
                                        var tags = video.tags;
                                        foreach (string tag in tags)
                                        {
                                            item.tags.Add((string)tag);
                                        }
                                    }
                                    cat.videos.Add(item);
                                }
                            }
                            archive.categories.Add(cat);
                        }




                    }
                }
            }
            catch { }
            ////Get KF Account Items
            try
            {
                var response = kfRequest.GetResponse();
                Stream dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();

                    if (BCResponseString != null && BCResponseString != "null")
                    {
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);

                        videoCategory category = new videoCategory();
                        category.name = results.name;
                        category.videos = new List<videoItem>();

                        foreach (dynamic video in results.videos)
                        {
                            videoItem item = new videoItem();
                            item.id = video.id;
                            item.name = video.name.ToString().Replace("\"", "&quot");
                            item.length = video.length;
                            item.playsTotal = video.playsTotal;
                            item.thumbnailURL = video.thumbnailURL;
                            item.videoStillURL = video.videoStillURL;
                            item.shortDescription = video.shortDescription.ToString().Replace("\"", "&quot");
                            if (video.tags != null)
                            {
                                item.tags = new List<string>();
                                var tags = video.tags;
                                foreach (string tag in tags)
                                {
                                    item.tags.Add((string)tag);
                                }
                            }
                            category.videos.Add(item);
                        }
                        archive.categories.Add(category);
                    }
                }
            }
            catch { }
            return archive;

        }
    }
}