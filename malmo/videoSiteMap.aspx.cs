using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Caching;
using System.Xml;
using System.Text;


namespace malmo
{
    public partial class videoSiteMap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Encoding = new UTF8Encoding(false);
            writerSettings.Indent = true;
            XmlWriter writer = XmlWriter.Create(Server.MapPath("videoSiteMap.xml"),writerSettings);
            
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset","http://www.sitemaps.org/schemas/sitemap/0.9");
            writer.WriteAttributeString("xmlns", "video", null, "http://www.google.com/schemas/sitemap-video/1.0");

            string pubId = "745456160001";
            string playId = "745456405001";

            videoArchive archive = (videoArchive)Cache["Archive"];
            if (archive == null) {
                buildVideoArchive builder = new buildVideoArchive();
                archive = builder.render(true);
            }
            if (archive != null) {                
                foreach (videoCategory category in archive.categories) {
                    foreach (videoItem item in category.videos) {
                        if (category.name == "Kommunfullmäktige") {
                            pubId = "2494809924001";
                            playId = "2821564386001";
                        }
                        writeTag(category.name, pubId, playId, item, writer);
                    }
                }
            }

            writer.WriteEndDocument();
            writer.Close();

            Response.Redirect("videoSiteMap.xml");

        }

        private void writeTag(string category, string publisherId, string playerId, videoItem item, XmlWriter w)
        {
            long milli;
            bool parse = long.TryParse(item.length, out milli);
            string duration = (milli / 1000).ToString();
            w.WriteStartElement("url");

            w.WriteElementString("loc", "http://video.malmo.se/?bctid=" + item.id);

            w.WriteStartElement("video","video",null);

            w.WriteElementString("video", "title", null, item.name);
            w.WriteElementString("video", "description", null, item.shortDescription);
            w.WriteElementString("video", "thumbnail_loc", null, item.videoStillURL);
            w.WriteElementString("video", "family_friendly", null, "Yes");
            w.WriteElementString("video", "category", null, category);
            w.WriteElementString("video", "view_count", null, item.playsTotal);
            w.WriteElementString("video", "duration", null, duration);

            w.WriteStartElement("video", "player_loc", null);
            w.WriteAttributeString("allow_embed", "true");
            w.WriteString("http://c.brightcove.com/services/viewer/federated_f9/" + playerId + "?isVid=1&isUI=1&domain=embed&playerID=" + playerId + "&videoID=" + item.id + "&publisherID=" + publisherId);
            w.WriteEndElement();




            w.WriteEndElement();

            w.WriteEndElement();

        }
    }
}