using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Caching;

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

    public class helpers
    {
        private Cache _cache = System.Web.HttpContext.Current.Cache;
        public helpers() { }

        #region KF Dropdown List
        public string KfDropdownList()
        {
            string list = (string)_cache["kfListString"];
            if (list == null)
            {
                list = renderKFDropDownList(getVideoArchive(false));
                _cache.Insert("kfListString", list, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
            }
            return list;
        }
        private string renderKFDropDownList(videoArchive archive)
        {
            StringBuilder html = new StringBuilder();
            videoCategory kfVideos = new videoCategory();
            if (archive != null)
            {
                foreach (videoCategory category in archive.categories)
                {
                    if (category.name == "Kommunfullmäktige")
                    {
                        kfVideos = category;
                    }
                }
            }
            System.Data.DataTable kfTable = new System.Data.DataTable("KF Videor");
            kfTable.Columns.Add("id", typeof(string));
            kfTable.Columns.Add("name", typeof(string));
            kfTable.Columns.Add("year", typeof(int));
            kfTable.Columns.Add("month", typeof(int));

            if (kfVideos != null)
            {
                foreach (videoItem video in kfVideos.videos)
                {
                    int month = 0;
                    int year = DateTime.Now.Year;
                    string id = "0";
                    string name = "KF Video";

                    if (video.id != null) { id = video.id; }
                    if (video.name != null) { name = video.name; }
                    if (video.tags != null)
                    {
                        foreach (string tag in video.tags)
                        {
                            //är det en månads-tagg
                            if (tag.ToUpper() == "JANUARI") { month = 1; }
                            if (tag.ToUpper() == "FEBRUARI") { month = 2; }
                            if (tag.ToUpper() == "MARS") { month = 3; }
                            if (tag.ToUpper() == "APRIL") { month = 4; }
                            if (tag.ToUpper() == "MAJ") { month = 5; }
                            if (tag.ToUpper() == "JUNI") { month = 6; }
                            if (tag.ToUpper() == "JULI") { month = 7; }
                            if (tag.ToUpper() == "AUGUSTI") { month = 8; }
                            if (tag.ToUpper() == "SEPTEMBER") { month = 9; }
                            if (tag.ToUpper() == "OKTOBER") { month = 10; }
                            if (tag.ToUpper() == "NOVEMBER") { month = 11; }
                            if (tag.ToUpper() == "DECEMBER") { month = 12; }

                            //eller en års-tag
                            string pattern = @"\d{4}";
                            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern);
                            if (r.IsMatch(tag))
                            {
                                year = Int32.Parse(tag);
                            }
                        }
                    }
                    kfTable.Rows.Add(id, name, year, month);
                }
            }
            //sortera sändningarna per år och månad
            System.Data.DataView dv = kfTable.DefaultView;
            dv.Sort = "year DESC, month DESC";

            //skapa en tabell över de unika åren 
            System.Data.DataTable yearsTable = dv.ToTable(true, "year");

            //bygg listan
            html.AppendLine("<select onChange=\"kfListChange()\" id=\"kfList\">");
            foreach (System.Data.DataRow r in yearsTable.Rows)
            {
                html.AppendLine("\t<optgroup label=\"" + r["year"].ToString() + "\">");
                foreach (System.Data.DataRow kfRow in kfTable.Rows)
                {
                    if (kfRow["year"].ToString() == r["year"].ToString())
                    {

                        html.AppendLine("\t\t<option value=\"" + kfRow["id"].ToString() + "\">" + kfRow["name"].ToString().Replace(kfRow["year"].ToString(), "") + "</option>");
                    }
                }
                html.AppendLine("\t</optgroup>");
            }
            html.AppendLine("</select>");

            return html.ToString();
        }
        #endregion

        public videoArchive getVideoArchive(bool komin)
        {
            buildVideoArchive builder = new malmo.buildVideoArchive();

            videoArchive archive = new videoArchive();
            if (komin)
            {
                archive = (videoArchive)_cache["kominArchive"];
                if (archive == null)
                {
                    archive = builder.render(true);
                    _cache.Insert("kominArchive", archive, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
                }
            }
            else
            {
                archive = (videoArchive)_cache["Archive"];
                if (archive == null)
                {
                    archive = builder.render(false);
                    _cache.Insert("Archive", archive, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
                }
            }

            return archive;
        }

        public string carouselHtml()
        {

            StringBuilder r = new StringBuilder();

            r.AppendLine("<div class=\"videocarousel\">");
            videoArchive archive = getVideoArchive(false);
            foreach (videoCategory cat in archive.categories)
            {
                if (cat.name == "Aktuellt")
                {
                    int counter = 0;
                    foreach (videoItem v in cat.videos)
                    {
                        r.AppendLine("<div>");
                        r.AppendLine("<a title=\"" + v.shortDescription + "\" href=\"?bctid=" + v.id + "\" tabindex=\"-1\">");
                        r.AppendLine("<figure>");
                        r.AppendLine("<img src=\"" + v.videoStillURL + "\" alt=\"" + v.name + "\" />");
                        r.AppendLine("</figure>");
                        r.AppendLine("<span class=\"videoinfo\"><span class=\"slick_title\">" + v.name + "</span><span class=\"slick_description\">" + v.shortDescription + "</span></span>");
                        r.AppendLine("</a>");
                        r.AppendLine("</div>");
                        counter++;
                        if (counter > 9) break;
                    }
                }
            }
            r.AppendLine("</div>");


            return r.ToString();
        }
    }
}