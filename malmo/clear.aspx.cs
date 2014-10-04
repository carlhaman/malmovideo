using System;

namespace malmo
{
    public partial class clear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string html = string.Empty;
            html += "\n<h2>Cache:</h2>";
            if (Cache["Archive"] != null) { html += "\n<p>Video archive is cached</p>"; }
            if (Cache["renderedArchive"] != null) { html += "\n<p>Video archive pre-render is cached</p>"; }

            if (Cache["kominArchive"] != null) { html += "\n<p>KOMIN Video archive is cached</p>"; }
            if (Cache["renderedKominArchive"] != null) { html += "\n<p>KOMIN Video archive pre-render is cached</p>"; }

            if (Cache["kfListString"] != null) { html += "\n<p>Kf-Dropdownlist is cached</p>"; }

            html += "\n<hr/>";

            html += "\n<h2>Video caches cleared:</h>";

            if (Cache["Archive"] != null)
            {
                Cache.Remove("Archive");
                if (Cache["Archive"] == null)
                {
                    html += "\n<p>Video archive cache is removed!</p>";
                }
            }
            if (Cache["kominArchive"] != null)
            {
                Cache.Remove("kominArchive");
                if (Cache["kominArchive"] == null)
                {
                    html += "\n<p>KOMIN Video archive cache is removed!</p>";
                }
            }

            if (Cache["kfListString"] != null)
            {
                Cache.Remove("kfListString");
                if (Cache["kfListString"] == null)
                {
                    html += "\n<p>KF drop down list cache removed!</p>";
                }
            }

            if (Cache["archiveHtml"] != null)
            {
                Cache.Remove("archiveHtml");
                if (Cache["archiveHtml"] == null)
                {
                    html += "\n<p>Video pre-render archive cache is removed!</p>";
                }
            }

            if (Cache["kominArchiveHtml"] != null)
            {
                Cache.Remove("kominArchiveHtml");
                if (Cache["kominArchiveHtml"] == null)
                {
                    html += "\n<p>KOMIN Video  pre-render archive cache is removed!</p>";
                }
            }

            if (Cache["latestVideoId"] != null)
            {
                Cache.Remove("latestVideoId");
                if (Cache["latestVideoId"] == null)
                {
                    html += "\n<p>Latest video ID removed</p>";
                }
            }

            if (Cache["latestKfVideoId"] != null)
            {
                Cache.Remove("latestKfVideoId");
                if (Cache["latestKfVideoId"] == null)
                {
                    html += "\n<p>Latest KF video ID removed</p>";
                }
            }



            pageContent.InnerHtml = html;
        }
    }
}