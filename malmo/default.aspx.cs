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
using System.Web.UI.HtmlControls;
using Newtonsoft;


namespace malmo
{
    public partial class index : System.Web.UI.Page
    {
        //private static string BCReadToken = "";
        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        private static string MReadToken = System.Configuration.ConfigurationManager.AppSettings["M_READ_URL_TOKEN"].ToString();

        

        protected void Page_Load(object sender, EventArgs e)
        {
            videoArchive archive = (videoArchive)Cache["archive"];
            if (archive == null) {
                archive = buildVideoArchive();
                Cache.Insert("archive", archive, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
            }

            //För att inte indexera staging-server
            if (!Request.Url.Host.ToString().Contains("video.malmo.se"))
            {
                HtmlMeta robotMeta = new HtmlMeta();
                robotMeta.Name = "ROBOTS";
                robotMeta.Content = "NOINDEX, NOFOLLOW";
                Page.Header.Controls.Add(robotMeta);
            }

            renderMasthead(); 

            string queryId = string.Empty;

            if (Request.QueryString["bctid"] != null)
            {
                queryId = Request.QueryString.GetValues("bctid").GetValue(0).ToString();
            }

            if (queryId.Length > 6)
            {
                getBrightcoveVideo(queryId, MReadToken);
            }
            else { getLatestVideo(); }

            renderVideoArchive(archive);

        }

        private void renderMasthead()
        {

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

        private void parseBrightcoveVideo(string BCResponseString)
        {

            JavaScriptSerializer js = new JavaScriptSerializer();

            if (BCResponseString != "null")
            {
                string metaHtml = string.Empty;
                VideoMeta meta = (VideoMeta)js.Deserialize(BCResponseString, typeof(VideoMeta));

                if (meta != null)
                {
                    string playerKey = string.Empty;
                    string playerId = string.Empty;
                    bool CBPlayer = false;

                    if (meta.customFields != null)
                    {
                        if (meta.customFields.ContainsKey("cblandingpage"))
                        {
                            CBPlayer = true;
                            playerKey = System.Configuration.ConfigurationManager.AppSettings["KF_BC_PLAYERKEY"].ToString();
                            playerId = System.Configuration.ConfigurationManager.AppSettings["KF_BC_PLAYERID"].ToString();

                            metaHtml += "<div class=\"embed-container\">\n";
                            metaHtml += "<iframe src=\"" + meta.customFields["cblandingpage"] + "\" frameborder=\"0\" allowfullscreen></iframe>\n";
                            metaHtml += "</div>\n";
                        }
                    }
                    if (!CBPlayer)
                    {
                        playerKey = System.Configuration.ConfigurationManager.AppSettings["M_BC_PLAYERKEY"].ToString();
                        playerId = System.Configuration.ConfigurationManager.AppSettings["M_BC_PLAYERID"].ToString();

                        metaHtml += "<div class=\"embed-container\">\n";
                        //metaHtml += "<iframe src=\"http://link.brightcove.com/services/player/bcpid745456405001?bckey=AQ~~,AAAArZCmTQE~,w5iz83926fkXk5wAB6K2HNZ2NUmtlRla&bctid=" + meta.id.ToString() + "\" frameborder=\"0\" allowfullscreen></iframe>\n";
                        //testar med javascript istället
                        metaHtml += @"

                                    <script language='JavaScript' type='text/javascript' src='http://admin.brightcove.com/js/BrightcoveExperiences.js'></script>

                                    <object id='myExperience" + meta.id.ToString() + @"' class='BrightcoveExperience'>
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
                    if (meta.id > 0)
                    {
                        metaOgUrl.Attributes["content"] = "http://video.malmo.se/?bctid=" + meta.id.ToString();
                        metaTwitterUrl.Attributes["content"] = "http://video.malmo.se/?bctid=" + meta.id.ToString();
                        string twitterPlayerUrl = "https://link.brightcove.com/services/player/bcpid" + playerId + "?bckey=" + playerKey + "bctid=" + meta.id.ToString() + "&secureConnections=true&autoStart=false&height=100%25&width=100%25";
                        metaTwitterPlayer.Attributes["content"] = twitterPlayerUrl;
                    }
                    if (meta.name != null)
                    {
                        metaHtml += "<h1>" + meta.name + "</h1>\n";
                        metaOgTitle.Attributes["content"] = meta.name;
                        metaTwitterTitle.Attributes["content"] = meta.name;
                        metaPageTitle.Text = "Malmö Stad Video - " + meta.name;
                    }
                    if (meta.shortDescription != null)
                    {
                        metaHtml += "<p>" + meta.shortDescription + "</p>\n";
                        metaOgDescription.Attributes["content"] = meta.shortDescription;
                        metaTwitterDescription.Attributes["content"] = meta.shortDescription;
                    }
                    if (meta.videoStillURL != null)
                    {
                        metaOgImage.Attributes["content"] = meta.videoStillURL;
                        metaTwitterImage.Attributes["content"] = meta.videoStillURL;
                    }

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

        private void getBrightcoveVideo(string brightcoveId, string token)
        {

            Stream dataStream;
            string requestFields = "id,name,shortDescription,publishedDate,tags,customFields,videoStillURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_video_by_id&video_id={0}&video_fields={1}&token={2}", brightcoveId, requestFields, token));
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
                        if (BCResponseString != "null")
                        {
                            parseBrightcoveVideo(BCResponseString);
                            getRelatedVideos(brightcoveId, token);
                        }
                        if (BCResponseString == "null" && token == MReadToken)
                        {
                            getBrightcoveVideo(brightcoveId, KFReadToken);
                        }
                    }

                }

            }
            catch (WebException ex) { }

        }
        private void setMetaOnPage(string name)
        {

        }

        private void getLatestVideo()
        {
            //Kf listan: 2623641282001
            //Aktuellt listan: 1172867907001
            bool isKf = false;
            if (Request.QueryString["kf"] != null)
            {
                isKf = true;
            }
            string token = MReadToken;
            string playlistId = "1172867907001";
            if (isKf) { token = KFReadToken; playlistId = "2623641282001"; }

            Stream dataStream;

            string videoFields = "id,name,shortDescription,publishedDate,tags,customFields,videoStillURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlist_by_id&playlist_id={0}&video_fields={1}&token={2}", playlistId, videoFields, token));
            request.Method = "POST";

            try
            {
                var response = request.GetResponse();
                dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;

                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();

                    if (BCResponseString != null && BCResponseString != "null")
                    {
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);
                        string id = results.videoIds[0];
                        if (id.Length > 6)
                        {
                            getBrightcoveVideo(id, token);
                        }

                    }
                }

            }
            catch (WebException ex) { }


        }

        private void getRelatedVideos(string brightcoveId, string token)
        {
            Stream dataStream;
            string html = string.Empty;

            string videoFields = "id,name,shortDescription,customFields,videoStillURL,thumbnailURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_related_videos&video_id={0}&video_fields={1}&token={2}", brightcoveId, videoFields, token));
            request.Method = "POST";

            try
            {
                var response = request.GetResponse();


                dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();

                    if (BCResponseString != null && BCResponseString != "null")
                    {
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);

                        html += "<dl class=\"accordion\">\n";
                        html += "<dt><h2>Relaterade Videos</h2></dt>\n";
                        html += "<ul class=\"video_grid\" style=\"display:none;\">\n";

                        foreach (dynamic video in results.items)
                        {
                            string title = video.name.ToString().Replace("\"", "&quot");
                            string description = video.shortDescription.ToString().Replace("\"", "&quot");

                            html += "\t\t<li class=\"video_item tooltip\" title=\"<h2>" + title + "</h2><img src='" + video.videoStillURL.ToString() + "' width='400' height='225'/><p>" + description + "</p>\" >\n";
                            html += "\t\t\t<a href=\"?bctid=" + video.id.ToString() + "\">\n";
                            html += "\t\t\t<img class=\"lazy\" src=\"Images/grey.gif\" data-original=\"" + video.thumbnailURL.ToString() + "\" width=\"160\" height=\"90\"/>\n";
                            html += "\t\t\t<h4>" + title + "</h4>\n";
                            html += "\t\t\t</a>";
                            html += "\t\t</li>\n";
                        }

                        html += "\t<li style=\"clear:both;\"</li>\n";
                        html += "</ul>\n";
                        html += "</dl>\n";


                    }
                }

            }
            catch (WebException ex) { }

            relatedVideos.InnerHtml = html;


        }

        private videoArchive buildVideoArchive()
        {
            videoArchive archive = new videoArchive();
            archive.categories = new List<videoCategory>();

            string kfPlaylistBcId = "2623641282001";
            string mArchivePlayerBcId = "1180742924001";
            string videoFields = "id,name,shortDescription,videoStillURL,thumbnailURL,length,playsTotal";

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
                                videoItem item = new videoItem();
                                item.id = video.id;
                                item.name = video.name.ToString().Replace("\"", "&quot");
                                item.length = video.length;
                                item.playsTotal = video.playsTotal;
                                item.thumbnailURL = video.thumbnailURL;
                                item.videoStillURL = video.videoStillURL;
                                item.shortDescription = video.shortDescription.ToString().Replace("\"", "&quot");
                                cat.videos.Add(item);
                            }
                            archive.categories.Add(cat);
                        }




                    }
                }
            }
            catch { }
            //Get KF Account Items
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
                            category.videos.Add(item);
                        }
                        archive.categories.Add(category);
                    }
                }
            }
            catch { }
            return archive;

        }

        private void renderVideoArchive(videoArchive archive) {

            string html = (string)Cache["archiveHtml"];
            if (html == null) {
                html += "<dl class=\"accordion\">\n";
                foreach (videoCategory category in archive.categories) {
                    html += "<dt><h2>"+category.name+"</h2></dt>";
                    html += "<ul class=\"video_grid\" style=\"display:none;\">\n";
                    foreach (videoItem video in category.videos) {
                        html += "<li class=\"video_item tooltip\" title=\"<h2>" + video.name + "</h2><img src='" + video.videoStillURL + "' width='400' height='225'/><p>" + video.shortDescription + "</p>\" >\n";
                        html += "\t<a href=\"?bctid="+video.id+"\">";
                        html += "<img class=\"lazy\" src=\"Images/grey.gif\" data-original=\"" + video.thumbnailURL.ToString() + "\" width=\"160\" height=\"90\"/>";
                        html += "<h4>"+video.name+"</h4>";
                        html += "</a>\n";
                        html += "</li>\n";
                    }
                    html += "<li style=\"clear:both;\"></li>\n";
                    html += "</ul>\n";
                }
                html += "</dl>\n";

                Cache.Insert("archiveHtml", html, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
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