using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;
using System.Xml;
using System.Text;
using System.Net;

namespace malmo
{
    public partial class rss : System.Web.UI.Page
    {
        private bool komin = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            ipRangeCheck malmo = new ipRangeCheck();
            komin = malmo.MalmoNetwork();

            string queryId = string.Empty;

            if (Request.QueryString["playlistId"] != null)
            {
                queryId = Request.QueryString.GetValues("playlistId").GetValue(0).ToString();
            }

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Encoding = new UTF8Encoding(false);
            writerSettings.Indent = true;

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, new UTF8Encoding(false));

            XmlWriter writer = XmlWriter.Create(sw, writerSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("rss");            
            writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("version", "2.0");

            if (queryId != "") { populateXML(queryId, writer); }

            writer.Flush();
            writer.Close();

            StreamReader sr = new StreamReader(ms);
            ms.Position = 0;
            string response = sr.ReadToEnd();


            Response.Clear();
            Response.ClearHeaders();
            Response.ContentType = "text/xml";
            Response.Charset = "UTF-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Write(response);
            Response.End();



        }

        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        private static string MReadToken = System.Configuration.ConfigurationManager.AppSettings["M_READ_URL_TOKEN"].ToString();

        private void populateXML(string playlistId, XmlWriter w) 
        {
            Stream dataStream;
            
            string videoFields = "id,name,shortDescription,publishedDate,customFields,videoStillURL";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlist_by_id&playlist_id={0}&video_fields={1}&token={2}", playlistId, videoFields, MReadToken));
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
                        w.WriteStartElement("channel");
                        w.WriteElementString("title", "Video > "+results.name.ToString());
                        w.WriteElementString("link", "http://video.malmo.se");
                        w.WriteElementString("description", "RSS-feed för spellistan " + playlistId);
                        w.WriteElementString("lastBuildDate", DateTime.Now.ToString("r"));

                        w.WriteStartElement("atom", "link", null);
                        w.WriteAttributeString("href", "http://video.malmo.se/rss.aspx?playlistId="+playlistId);
                        w.WriteAttributeString("rel", "self");
                        w.WriteAttributeString("type", "application/rss+xml");
                        
                        w.WriteEndElement();

                            foreach (dynamic video in results.videos)
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
                                    w.WriteStartElement("item");
                                    w.WriteElementString("title", video.name.ToString().Replace("\"", "&quot"));
                                    w.WriteElementString("description", video.shortDescription.ToString().Replace("\"", "&quot"));
                                    w.WriteElementString("link", "http://video.malmo.se/?bctid=" + video.id.ToString());
                                    w.WriteElementString("pubDate", BrightcovePublishedToDateTime(video.publishedDate.ToString()).ToString("r"));
                                    if (video.videoStillURL.ToString() != null)
                                    {
                                        w.WriteStartElement("enclosure");
                                        w.WriteAttributeString("url", video.videoStillURL.ToString());
                                        w.WriteAttributeString("type", "image/jpeg");
                                        w.WriteEndElement();
                                    }
                                    
                                    
                                    w.WriteEndElement();                                   
                                }
                            }
                        }
                    }
                

            }
            catch (WebException ex) { }
        }
        private DateTime BrightcovePublishedToDateTime(string milliseconds) {
            DateTime UNIXepoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long milli;
            bool parse = long.TryParse(milliseconds, out milli);
            if (parse) { UNIXepoch = UNIXepoch.AddMilliseconds(milli); } 
            return UNIXepoch;
        }
    }

   
}