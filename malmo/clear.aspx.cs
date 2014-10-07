using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;

namespace malmo
{
    public partial class clear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HtmlMeta robotMeta = new HtmlMeta();
            robotMeta.Name = "ROBOTS";
            robotMeta.Content = "NOINDEX, NOFOLLOW";
            Page.Header.Controls.Add(robotMeta);

            string html = string.Empty;

            if (!string.IsNullOrEmpty(Request.QueryString["delete"]))
            {
                string action = HttpUtility.UrlDecode(Request.QueryString.GetValues("delete").GetValue(0).ToString());
                switch (action)
                {
                    case "all":
                        string all = emptyCache();
                        if (all.Length > 1) { html += "<div class=\"deleted\">" + all + "</div>"; }
                        break;
                    default:
                        string result = deleteFromCache(action);
                        if (result.Length > 1) { html += "<div class=\"deleted\">" + result + "</div>"; }
                        break;
                }
            }

            html += test();
            pageContent.InnerHtml = html;
        }

        private Dictionary<string, string> cacheValues()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Archive", "Public video archive");
            dict.Add("renderedArchive", "Public video archive HTML");
            dict.Add("kominArchive", "Internal video archive");
            dict.Add("renderedKominArchive", "Internal video archive HTML");
            dict.Add("kfListString", "KF dropdown list HTML");
            dict.Add("carousel", "Start page carousel HTML");
            dict.Add("latestVideoId", "Latest video ID from Brightcove");
            dict.Add("latestKfVideoId", "Latest KF video ID from Brightcove");
            return dict;
        }

        private string test()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class=\"currentCache\"><h1>Items in cache</h1>");
            int counter = 0;
            foreach (KeyValuePair<string, string> k in cacheValues())
            {

                if (Cache[k.Key] != null)
                {
                    sb.AppendLine("<p><span>" + k.Value + "</span><a href=\"?delete=" + k.Key + "\">Delete</a></p>");
                    counter++;
                }

            }
            if (counter >= 2) { sb.AppendLine("<p style=\"text-align: center;\"><a href=\"?delete=all\">Empty Cache</a></p>"); }
            if (counter == 0) { sb.AppendLine("<p>No items in cache!</p>"); }
            sb.AppendLine("</div>");
            return sb.ToString();
        }
        private string deleteFromCache(string key)
        {
            string response = string.Empty;

            string description = string.Empty;
            if (cacheValues().TryGetValue(key, out description))
            {
                if (Cache[key] != null)
                {
                    Cache.Remove(key);
                    if (Cache[key] == null)
                    {
                        response = "<p>" + description + " - Removed from Cache!</p>";
                    }
                }
            }
            return response;
        }
        private string emptyCache()
        {
            string response = string.Empty;
            foreach (KeyValuePair<string, string> k in cacheValues())
            {
                if (Cache[k.Key] != null)
                {
                    Cache.Remove(k.Key);
                    if (Cache[k.Key] == null)
                    {
                        response += "<p>" + k.Value + " - Removed from cache!</p>";
                    }
                }
            }
            return response;
        }
    }
}