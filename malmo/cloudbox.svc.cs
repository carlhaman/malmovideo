using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json.Linq;
using ungap.CloudBox;

namespace malmo
{
    public class cloudbox : Icloudbox
    {
        public System.IO.Stream get_smil(string video_id)
        {
            try {
                return private_get_smil(video_id);
            }
            catch (Exception ex) { 
            }

            // TODO: Handle error
            return new System.IO.MemoryStream(ASCIIEncoding.Default.GetBytes("ERROR: get_smil failed"));
        }

        private System.IO.Stream private_get_smil(string video_id)
        {
            string bc_read_token = ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"];

            // Set MIME type for returned data
            // http://stackoverflow.com/questions/992533/wcf-responseformat-for-webget
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/smil";

            // Get video streams from video ID
            string url = "http://api.brightcove.com/services/library?command=find_video_by_id&video_id={1}&video_fields=id,FLVURL,customFields,HLSURL&media_delivery=default&token={0}";

            // Insert BC READ TOKEN to the URL
            url = String.Format(url, bc_read_token, video_id);
            string json = "";

            // Use WebClient to read data from service. Synchronous call.
            WebClient webClient = new WebClient();
            json = webClient.DownloadString(url);

            // Parsing the Json with Linq

            JObject jo = JObject.Parse(json);

            // Create SMIL object
            Videos.Smil smil = new Videos.Smil();

            smil.Base = jo["customFields"]["cb-rtmp-streamer"].ToString();

            string starttime = jo["customFields"]["cb-rec-starttime"].ToString();

            // Parse the date string
            DateTime dtStarttime = DateTime.Now;
            DateTime.TryParse(starttime, out dtStarttime);

            string files = jo["customFields"]["cb-rtmp-files"].ToString();
            // Parse files
            string[] parts = files.Split('\r');
            foreach (string part in parts)
            {
                if (part.Length == 0) continue;
                ungap.CloudBox.Video video = new ungap.CloudBox.Video(files, 0, "video/flash", "brightcove:" + video_id, dtStarttime, 0, true);
                smil.videos.Add(video);
            }

            string strSmil = smil.toXml();
            return new System.IO.MemoryStream(ASCIIEncoding.Default.GetBytes(strSmil));
        }

        /* {
          "id": 2624888559001,
          "customFields": {
            "cb-landing-page": "http://malmostad.wallenmedia.se/kf20130321/",
            "cb-list": "1111111111111",
            "cb-rtmp-url": "rtmp://fl1.c04321.cdn.qbrick.com/04321&malmoe_stad_130618.f4v"
          },
          "FLVURL": "rtmp://brightcove.fcod.llnwd.net/a500/d17/&mp4:media/2494809924001/2494809924001_2625102565001_kf20130321.mp4&1377612000000&27d9762c99a85f8e9df658b72c88f0a8",
          "HLSURL": null,
          "IOSRenditions": []
        }
        */
    }
}
