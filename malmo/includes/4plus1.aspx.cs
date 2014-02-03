using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace malmo.includes
{
    public partial class _4plus1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            videoArchive archive = (videoArchive)Cache["Archive"];
            if (archive == null)
            {
                buildVideoArchive builder = new buildVideoArchive();
                archive = builder.render(false);
                Cache.Insert("Archive", archive, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
            }
            foreach (videoCategory cat in archive.categories)
            {
                if (cat.name == "Aktuellt") { renderAktuellt(cat); }
                if (cat.name == "Kommunfullmäktige") { renderKF(cat); }
            }

        }
        private void renderKF(videoCategory category)
        {
            int counter = 0;
            StringBuilder html = new StringBuilder();

            html.AppendLine("<div class=\"header\"><h1>Kommunfullmäktige</h1></div>");
            foreach (videoItem clip in category.videos)
            {
                html.Append(renderClip(clip, true));
                counter++;
                if (counter >= 1) { break; };
            }
            kfContainer.InnerHtml = html.ToString();
        }
        private void renderAktuellt(videoCategory category)
        {
            int counter = 0;
            StringBuilder html = new StringBuilder();

            html.AppendLine("<div class=\"header\"><h1>Webbvideo</h1></div>");

            foreach (videoItem clip in category.videos)
            {
                html.Append(renderClip(clip, false));
                counter++;
                if (counter >= 4) { break; };
            }
            html.AppendLine("<div style=\"clear:both;\"></div>");
            aktuelltContainer.InnerHtml = html.ToString();
        }
        private string renderClip(videoItem clip, bool kf)
        {
            string html = string.Empty;
            html += "<a href=\"http://video.malmo.se/?bctid=" + clip.id + "\" alt=\"" + clip.shortDescription + "\">\n";
            html += "<div class=\"clip\">\n";
            html += "\t<div class=\"pics\">\n";
            if (!kf) { html += "\t\t<p class=\"time\">" + new TimeSpan(0, 0, 0, 0, Convert.ToInt32(clip.length)).ToString(@"mm\:ss", System.Globalization.CultureInfo.InvariantCulture) + "</p>\n"; }
            if (kf) { html += "\t\t<p class=\"time\">" + new TimeSpan(0, 0, 0, 0, Convert.ToInt32(clip.length)).ToString(@"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture) + "</p>\n"; }
            html += "\t\t<img class=\"overlay\" src=\"" + Request.Url.GetLeftPart(UriPartial.Authority) + "/Images/video_play_overlay.png\"/>\n";
            html += "\t\t<img src=\"" + clip.thumbnailURL + "\"/>\n";
            html += "\t</div>\n";
            html += "<p class=\"desc\">" + clip.name + "</p>\n";
            html += "</div>\n";
            html += "</a>\n";
            return html;
        }

    }
}