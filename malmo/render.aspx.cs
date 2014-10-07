using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace malmo
{
    public partial class render : System.Web.UI.Page
    {
        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        private static string MReadToken = System.Configuration.ConfigurationManager.AppSettings["M_READ_URL_TOKEN"].ToString();
        private string _bctid = string.Empty;
        private bool _komin = false;
        StringBuilder htmlResponse = new StringBuilder();

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Parse page queries

            if (!string.IsNullOrEmpty(Request.QueryString["index"]))
            {
                string index = HttpUtility.UrlDecode(Request.QueryString.GetValues("index").GetValue(0).ToString().ToUpper());
                if (index == "KOMIN")
                {
                    _komin = true;
                }
            }

            if (!string.IsNullOrEmpty(Request.QueryString["bctid"]))
            {
                _bctid = HttpUtility.UrlDecode(Request.QueryString.GetValues("bctid").GetValue(0).ToString().ToUpper());
            }

            if (!string.IsNullOrEmpty(Request.QueryString["action"]))
            {
                string action = HttpUtility.UrlDecode(Request.QueryString.GetValues("action").GetValue(0).ToString().ToUpper());
                switch (action)
                {
                    case "RELATED":
                        if (!string.IsNullOrEmpty(_bctid)) { getRelatedVideos(_bctid); }
                        break;
                    case "RENDER":
                        renderArchive(_komin);
                        break;
                    default:
                        break;
                }
            }
            #endregion

            responseContent.InnerHtml = htmlResponse.ToString();
        }

        private void getRelatedVideos(string brightcoveId)
        {
            string cacheName = "rel" + brightcoveId + _komin.ToString().ToLower();
            string responseHtml = (string)Cache[cacheName];

            if (responseHtml == null)
            {
                string token = MReadToken;
                Stream dataStream;

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
                            StringBuilder htmlBuilder = new StringBuilder();

                            int counter = 0;
                            foreach (dynamic video in results.items)
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
                                if (counter <= 6)
                                {
                                    if (!kominVideo || kominVideo && _komin)
                                    {
                                        string title = video.name.ToString().Replace("\"", "&quot");
                                        string description = video.shortDescription.ToString().Replace("\"", "&quot");

                                        htmlBuilder.AppendLine("\t\t<div class=\"video_item\">\n");
                                        htmlBuilder.AppendLine("\t\t\t<a href=\"?bctid=" + video.id.ToString() + "\">\n");
                                        htmlBuilder.AppendLine("\t\t\t<div class=\"thumbnail\">\n");
                                        htmlBuilder.AppendLine("\t\t\t<img src=\"" + video.videoStillURL.ToString() + "\"/>\n");
                                        htmlBuilder.AppendLine("\t\t\t</div>\n");
                                        htmlBuilder.AppendLine("\t\t\t<div class=\"description\">\n");
                                        htmlBuilder.AppendLine("\t\t\t\t<h4>" + title + "</h4>\n");
                                        htmlBuilder.AppendLine("\t\t\t\t<p>" + description + "</p>\n");
                                        htmlBuilder.AppendLine("\t\t\t</div>\n");
                                        htmlBuilder.AppendLine("\t\t\t</a>");
                                        htmlBuilder.AppendLine("\t\t</div>\n");
                                        counter++;
                                    }
                                }
                            }

                            responseHtml = htmlBuilder.ToString();
                            if (counter > 1)
                            {

                                Cache.Insert(cacheName, responseHtml, null, DateTime.UtcNow.AddHours(6), Cache.NoSlidingExpiration);
                            }

                        }
                    }

                }
                catch (WebException ex) { }
            }
            htmlResponse.Append(responseHtml);
        }
        private void renderArchive(bool komin)
        {
            string renderName = "renderedArchive";
            if (komin) renderName = "renderedKominArchive";

            string response = (string)Cache[renderName];

            if (response == null)
            {
                helpers helper = new helpers();
                videoArchive archive = helper.getVideoArchive(komin);

                StringBuilder h = new StringBuilder();
                int categoryId = 1;
                foreach (videoCategory cat in archive.categories)
                {
                    h.AppendLine("<section class=\"va-group\" id=\"" + categoryId.ToString() + "\">");
                    h.AppendLine("<div class=\"va-video-list\">");
                    h.AppendLine("<div class=\"va-videolist-sectionheader\">");
                    h.AppendLine("<h2>" + cat.name + "</h2>");
                    h.AppendLine("<button class=\"expand-button\" tabindex=\"0\"><span>Visa alla</span></button>");
                    h.AppendLine("</div>");
                    h.AppendLine("<div class=\"va-videolist-container-outer\">");
                    h.AppendLine("<button class=\"slidearrow is-left is-invisible\" tabindex=\"-1\"></button>");
                    h.AppendLine("<button class=\"slidearrow is-right is-visible\" tabindex=\"-1\"></button>");
                    h.AppendLine("<div class=\"va-videolist-container not-expanded\">");
                    foreach (videoItem v in cat.videos)
                    {
                        h.AppendLine("<article>");
                        h.AppendLine("<a href=\"?bctid=" + v.id + "\" tabindex=\"-1\" title=\"" + v.shortDescription + "\">");
                        h.AppendLine("<figure class=\"va-thumb-cont\"><img src=\"" + v.thumbnailURL + "\" title=\"" + v.name + "\" class=\"va-thumb\" /></figure>");
                        h.AppendLine("<span class=\"va-title\">" + v.name + "</span>");
                        h.AppendLine("</a>");
                        h.AppendLine("</article>");

                    }
                    h.AppendLine("</div>");
                    h.AppendLine("</div>");
                    h.AppendLine("</div>");
                    h.AppendLine("</section>");
                    categoryId++;
                }
                response = h.ToString();
                if (categoryId > 3)
                {
                    Cache.Insert(renderName, response, null, DateTime.UtcNow.AddHours(6), Cache.NoSlidingExpiration);
                }
            }
            htmlResponse.Append(response);
        }

    }
}