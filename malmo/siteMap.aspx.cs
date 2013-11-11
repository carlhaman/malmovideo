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
    public partial class siteMap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Encoding = new UTF8Encoding(false);
            writerSettings.Indent = true;
            XmlWriter writer = XmlWriter.Create(Server.MapPath("siteMap.xml"), writerSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            videoArchive archive = (videoArchive)Cache["Archive"];
            if (archive == null)
            {
                buildVideoArchive builder = new buildVideoArchive();
                archive = builder.render(false);
            }
            if (archive != null)
            {
                foreach (videoCategory category in archive.categories)
                {
                    string priority = "0.5";
                    if (category.name == "Aktuellt") { priority = "1.0"; }

                    foreach (videoItem item in category.videos)
                    {
                        writeTag(item, writer, priority);
                    }
                }
            }

            writer.WriteEndDocument();
            writer.Close();

            Response.Redirect("siteMap.xml");


        }

        private void writeTag(videoItem item, XmlWriter w, string priority) 
        {
            w.WriteStartElement("url");

            w.WriteElementString("loc", "http://video.malmo.se/?bctid=" + item.id);
            w.WriteElementString("changefreq", "monthly");
            w.WriteElementString("priority", priority);

            w.WriteEndElement();
        }
    }
}