using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Caching;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;


namespace malmo
{
    public partial class index : System.Web.UI.Page
    {
        private static string BCReadToken = "xVURYsAXi4CfTPNEHuuIU28Fe1ll855bbtr1JbryM2HI9WUrBpjJ8w..";
        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        protected void Page_Load(object sender, EventArgs e)
        {
            
            renderMasthead();

            long brightcoveId = 0;
            bool queryId = false;

            if (Request.QueryString["bctid"] != null)
            {
                queryId = long.TryParse(Request.QueryString.GetValues("bctid").GetValue(0).ToString(), out brightcoveId);
            }

            if (queryId)
            {
                getBrightcoveVideo(brightcoveId);
            }

            getArchivePlayerItems(1180742924001);

        }

        private void renderMasthead() {

            string mastHeadString = (string)Cache["masthead"];
            if (mastHeadString == null)
            {
                var mastHeadRequest = (HttpWebRequest)HttpWebRequest.Create("http://www.malmo.se/assets-2.0/remote/external-masthead/");
                try
                {
                    var mastHeadResponse = mastHeadRequest.GetResponse();
                    Stream mastHeadStream;
                    mastHeadStream = mastHeadResponse.GetResponseStream();
                    using (StreamReader reader = new StreamReader(mastHeadStream))
                    {
                        mastHeadString = reader.ReadToEnd();
                        Cache.Insert("masthead", mastHeadString, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20));
                    }

                }
                catch (WebException ex) { }
            }
            mastHead.InnerHtml = mastHeadString;

        }

        private void getBrightcoveVideo(long brightcoveId)
        {

            Stream dataStream;
            string requestFields = "id,name,shortDescription,publishedDate,tags,customFields,videoStillURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_video_by_id&video_id={0}&video_fields={1}&token={2}", brightcoveId.ToString(), requestFields, KFReadToken));
            request.Method = "POST";

            try
            {
                var response = request.GetResponse();


                dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();
                    JavaScriptSerializer js = new JavaScriptSerializer();

                    if (BCResponseString != null)
                    {
                        string metaHtml = string.Empty;
                        VideoMeta meta = (VideoMeta)js.Deserialize(BCResponseString, typeof(VideoMeta));

                        if (meta != null)
                        {
                            bool CBPlayer = false;
                            if (meta.customFields != null)
                            {
                                if (meta.customFields.ContainsKey("cb-landing-page"))
                                {
                                    CBPlayer = true;
                                    metaHtml += "<div class=\"embed-container\">\n";
                                    metaHtml += "<iframe src=\"" + meta.customFields["cb-landing-page"] + "\" frameborder=\"0\" allowfullscreen></iframe>\n";
                                    metaHtml += "</div>\n";
                                }
                            }
                            if (!CBPlayer) {
                                metaHtml += "<div class=\"embed-container\">\n";
                                //metaHtml += "<iframe src=\"http://link.brightcove.com/services/player/bcpid745456405001?bckey=AQ~~,AAAArZCmTQE~,w5iz83926fkXk5wAB6K2HNZ2NUmtlRla&bctid=" + meta.id.ToString() + "\" frameborder=\"0\" allowfullscreen></iframe>\n";
                                //testar med javascript istället
                                metaHtml += @"

                                    <script language='JavaScript' type='text/javascript' src='http://admin.brightcove.com/js/BrightcoveExperiences.js'></script>

                                    <object id='myExperience"+meta.id.ToString() + @"' class='BrightcoveExperience'>
                                      <param name='bgcolor' value='#FFFFFF' />
                                      <param name='width' value='480' />
                                      <param name='height' value='270' />
                                      <param name='playerID' value='745456405001' />
                                      <param name='playerKey' value='AQ~~,AAAArZCmTQE~,w5iz83926fkXk5wAB6K2HNZ2NUmtlRla' />
                                      <param name='isVid' value='true' />
                                      <param name='isUI' value='true' />
                                      <param name='dynamicStreaming' value='true' />

                                      <param name='@videoPlayer' value='" + meta.id.ToString() + @"' />
                                    </object>
                                    <script type='text/javascript'>brightcove.createExperiences();</script>

                                    <!-- End of Brightcove Player -->
                                    ";


                                metaHtml += "</div>\n";
                            
                            }
                            if (meta.name != null) { metaHtml += "<h1>" + meta.name.ToString() + "</h1>\n"; }
                            if (meta.shortDescription != null) { metaHtml += "<p>" + meta.shortDescription + "</p>\n"; }
                            if (meta.length > 0) { metaHtml += "Längd: " + new TimeSpan(0, 0, 0, 0, (int)meta.length).ToString(@"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture) + "<br/>"; }
                            if (meta.publishedDate != null)
                            {
                                DateTime UNIXepoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                long milli;
                                bool parse = long.TryParse(meta.publishedDate, out milli);
                                if (parse) { UNIXepoch = UNIXepoch.AddMilliseconds(milli); }
                                metaHtml += "Publicerad: " + UNIXepoch.ToShortDateString() + "<br/>\n";
                            }
                            if (meta.playsTotal != null) { metaHtml += "Visad: " + meta.playsTotal + "\n"; }
                            if (meta.tags != null)
                            {
                                metaHtml += "<ul class=\videoTags\">\n";
                                foreach (string s in meta.tags)
                                {
                                    metaHtml += "<li>" + s + "</li>";
                                }
                                metaHtml += "</ul>\n";
                            }
                        }
                        string metadata = string.Empty;
                        videoDetails.InnerHtml = metaHtml;
                    }
                }

            }
            catch (WebException ex) { }

        }
        private void getArchivePlayerItems(long playerBcId)
        {
            string html = (string)Cache["archiveString"];
           
            if (html == null)
            {
                long playlistBcId = 2623641282001;
                playerBcId = 2619227676001;
                Stream dataStream;
                string videoFields = "id,name,shortDescription,videoStillURL,thumbnailURL,length,playsTotal";
                //var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlists_for_player_id&player_id={0}&video_fields={1}&token={2}", playerBcId.ToString(), videoFields, BCReadToken));
                var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlist_by_id&playlist_id={0}&video_fields={1}&token={2}", playlistBcId.ToString(), videoFields, KFReadToken));
                request.Method = "POST";

                try
                {
                    var response = request.GetResponse();


                    dataStream = response.GetResponseStream();
                    string BCResponseString = string.Empty;
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        BCResponseString = reader.ReadToEnd();

                        if (BCResponseString != null)
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            //RootObject parsedRoot = (RootObject)js.Deserialize(BCResponseString, typeof(RootObject));
                            //RootObject parsedRoot = JsonConvert.DeserializeObject<RootObject>(BCResponseString);
                            RootObject parsedRoot = new RootObject();
                            Item playlist = (Item)js.Deserialize(BCResponseString, typeof(Item));
                            parsedRoot.items = new List<Item>();
                            parsedRoot.items.Add(playlist);

                            html += "<div id=\"accordion\">\n";
                            foreach (Item i in parsedRoot.items)
                            {
                                html += "<h3>" + i.name.ToString() + "</h3>\n";
                                html += "<ul class=\"video_grid\" id=\"" + i.id.ToString() + "\">\n";

                                foreach (Video v in i.videos)
                                {
                                    html += "\t\t<a href=\"?bctid=" + v.id.ToString() + "\"><li class=\"video_item\">\n";                                    
                                    html += "\t\t\t<img src=\"" + v.thumbnailURL.ToString() + "\" width=\"120\" height=\"90\" alt=\"" + v.shortDescription.ToString() + "\"/>\n";
                                    //html += "\t\t\t<img class=\"lazy\" data-original=\"" + v.thumbnailURL.ToString() + "\" src=\"Images/grey.gif\" width=\"120\" height=\"90\" alt=\"" + v.shortDescription.ToString() + "\"/>\n";
                                    html += "\t\t\t" + v.name.ToString() + "\n";
                                    html += "\t\t</li></a>\n";

                                }

                                html += "\t<li style=\"clear:both;\"</li>\n";
                                html += "</ul>\n";
                            }
                            html += "</div>\n";

                            Cache.Insert("archiveString", html, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
                        }
                    }

                }
                catch (WebException ex) { }
            }
            videoArchive.InnerHtml = html;

        }
    }
    public class VideoMeta
    {
        public long id { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public string publishedDate { get; set; }
        public List<string> tags { get; set; }
        public string videoStillURL { get; set; }
        public int length { get; set; }
        public string playsTotal { get; set; }
        public Dictionary<string, string> customFields { get; set; }
    }
    public class Video
    {
        public long id { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public string videoStillURL { get; set; }
        public string thumbnailURL { get; set; }
        public int length { get; set; }
        public string playsTotal { get; set; }
    }
    public class Item
    {
        public long id { get; set; }
        public string referenceId { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public List<long> videoIds { get; set; }
        public List<Video> videos { get; set; }
        public string thumbnailURL { get; set; }
        public List<string> filterTags { get; set; }
        public string playlistType { get; set; }
    }
    public class RootObject
    {
        public List<Item> items { get; set; }
        public int page_number { get; set; }
        public int page_size { get; set; }
        public int total_count { get; set; }
    }
}