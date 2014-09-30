using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace malmo
{
    public partial class render : System.Web.UI.Page
    {
        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        private static string MReadToken = System.Configuration.ConfigurationManager.AppSettings["M_READ_URL_TOKEN"].ToString();
        private string _bctid = string.Empty;
        private bool _komin = false;

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
                _bctid = HttpUtility.UrlDecode(Request.QueryString.GetValues("index").GetValue(0).ToString().ToUpper());
            }

            if (!string.IsNullOrEmpty(Request.QueryString["action"]))
            {
                string action = HttpUtility.UrlDecode(Request.QueryString.GetValues("action").GetValue(0).ToString().ToUpper());
                switch (action)
                {
                    case "RELATED":
                        break;
                    default:
                        break;
                }
            }
            #endregion
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
                        StringBuilder htmlBuilder = new StringBuilder();

                        htmlBuilder.AppendLine("<dl>\n");
                        htmlBuilder.AppendLine("<dt><h2>Relaterade Videor</h2></dt>\n");
                        //htmlBuilder.AppendLine("<div id=\"scrollbar\" class=\"related\">\n");
                        htmlBuilder.AppendLine("<ul class=\"related_videos_list\">\n");
                        //html += "<dl class=\"accordion\">\n";
                        //html += "<dt><h2>Relaterade Videos</h2></dt>\n";
                        //html += "<ul class=\"video_grid\" style=\"display:none;\">\n";
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
                            if (counter <= 9)
                            {
                                if (!kominVideo || kominVideo && _komin)
                                {
                                    string title = video.name.ToString().Replace("\"", "&quot");
                                    string description = video.shortDescription.ToString().Replace("\"", "&quot");

                                    //htmlBuilder.AppendLine("\t\t<li class=\"video_item tooltip\" title=\"<h2>" + title + "</h2><img src='" + video.videoStillURL.ToString() + "' width='400' height='225'/><p>" + description + "</p>\" >\n");
                                    htmlBuilder.AppendLine("\t\t<li class=\"video_item\">\n");
                                    //htmlBuilder.AppendLine("\t\t<div class=\"info-box\"><h2>" + title + "</h2><img src='" + video.videoStillURL.ToString() + "' width='400' height='225'/><p>" + description + "</p></div>\n");
                                    htmlBuilder.AppendLine("\t\t\t<a href=\"?bctid=" + video.id.ToString() + "\">\n");
                                    htmlBuilder.AppendLine("\t\t\t<div class=\"thumbnail\">\n");
                                    htmlBuilder.AppendLine("\t\t\t<img src=\"" + video.videoStillURL.ToString() + "\" width=\"160\" height=\"90\"/>\n");
                                    htmlBuilder.AppendLine("\t\t\t</div>\n");
                                    htmlBuilder.AppendLine("\t\t\t<div class=\"description\">\n");
                                    htmlBuilder.AppendLine("\t\t\t\t<h4>" + title + "</h4>\n");
                                    htmlBuilder.AppendLine("\t\t\t\t<p>" + description + "</p>\n");
                                    htmlBuilder.AppendLine("\t\t\t</div>\n");
                                    htmlBuilder.AppendLine("\t\t\t</a>");
                                    htmlBuilder.AppendLine("\t\t</li>\n");
                                    counter++;
                                }
                            }
                        }

                        htmlBuilder.AppendLine("\t<li style=\"clear:both;\"</li>\n");
                        htmlBuilder.AppendLine("</ul>\n");
                        //htmlBuilder.AppendLine("</div>\n");
                        htmlBuilder.AppendLine("</dl>\n");

                        html = htmlBuilder.ToString();

                    }
                }

            }
            catch (WebException ex) { }

            //relatedVideos.InnerHtml = html;
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "lazy-loader", "<script type='text/javascript'>$('#relatedVideos').find('img.lazy').lazyload();</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "tooltip", "<script type='text/javascript'>$('#relatedVideos').find('.tooltip').tooltipster('destroy');$('#relatedVideos').find('.tooltip').tooltipster({theme: '.tooltipster-shadow',delay: 100,maxWidth: 420,touchDevices: false});</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "tooltip", "<script type='text/javascript'>$('#archiveContent').find('.tooltip').tooltipster({theme: '.tooltipster-shadow',delay: 100,maxWidth: 420,touchDevices: false});</script>", false);

        }


    }
}