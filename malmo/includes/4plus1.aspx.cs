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
        private int itemNumber = 1;

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

            html.AppendLine("<h2>Kommunfullmäktige</h2>\n");
            html.AppendLine("<ul>\n");
            foreach (videoItem clip in category.videos)
            {
                html.Append(renderClip(clip, true));
                counter++;
                if (counter >= 1) { break; };
            }
            html.AppendLine("</ul>\n");
            kfContainer.InnerHtml = html.ToString();
        }
        private void renderAktuellt(videoCategory category)
        {
            int counter = 0;
            StringBuilder html = new StringBuilder();

            html.AppendLine("<h1>Webbvideo</h1>\n");
            html.AppendLine("<ul>\n");
            foreach (videoItem clip in category.videos)
            {
                html.Append(renderClip(clip, false));
                counter++;
                if (counter >= 4) { break; };
            }
            html.AppendLine("</ul>\n");
            aktuelltContainer.InnerHtml = html.ToString();
        }
        private string renderClip(videoItem clip, bool kf)
        {
            string html = string.Empty;
            html += "<li class=\"item-"+itemNumber.ToString()+"\">\n";
            html += "\t<a href=\"http://video.malmo.se/?bctid=" + clip.id + "\" alt=\"" + clip.shortDescription + "\">\n";
            html += "\t\t<img src=\"" + clip.thumbnailURL + "\"/>\n";
            if (Convert.ToInt32(clip.length) >= 0)
            {
                if (!kf) { html += "\t\t<div class=\"video-time\">" + new TimeSpan(0, 0, 0, 0, Convert.ToInt32(clip.length)).ToString(@"mm\:ss", System.Globalization.CultureInfo.InvariantCulture) + "</div>\n"; }
                if (kf) { html += "\t\t<div class=\"video-time\">" + new TimeSpan(0, 0, 0, 0, Convert.ToInt32(clip.length)).ToString(@"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture) + "</div>\n"; }
            }
            html += "\t\t<h3>" + clip.name + "</h3>\n";
            html += "\t</a>\n";
            html += "</li>\n";

            itemNumber++;
            //html += "\t\t<img class=\"overlay\" src=\"" + Request.Url.GetLeftPart(UriPartial.Authority) + "/Images/video_play_overlay.png\"/>\n";
            return html;
        }

    }
}