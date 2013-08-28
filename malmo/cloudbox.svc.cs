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

        /// <summary>
        /// Get Json data from Brightcove API
        /// http://docs.brightcove.com/en/video-cloud/media/reference.html
        /// </summary>
        /// <param name="video_id">ID for the video to lookup</param>
        /// <returns>Json data from Brightcove API</returns>
        
        private string GetJson(string video_id)
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

            return json;
        }

        private System.IO.Stream private_get_smil(string video_id)
        {
            // Get video info from video ID
            string json = GetJson(video_id);

            // Parsing the Json with Linq
            JObject jo = JObject.Parse(json);

            // Create SMIL object
            Videos.Smil smil = new Videos.Smil();
            ungap.CloudBox.Video video = null;

            // Get attribute values from Json
            smil.Base = jo["customFields"]["cb-rtmp-streamer"].ToString();
            string starttime = jo["customFields"]["cb-rec-starttime"].ToString();

            // Parse the date string
            DateTime dtStarttime = DateTime.Now;
            DateTime.TryParse(starttime, out dtStarttime);

            // Parse files (RTMP)
            string files = jo["customFields"]["cb-rtmp-files"].ToString();
            string[] parts = files.Split('\r');
            foreach (string part in parts)
            {
                if (part.Length == 0) continue;
                video = new ungap.CloudBox.Video(part, 0, "video/flash", "brightcove:" + video_id, dtStarttime, 0, true);
                smil.videos.Add(video);
            }

            // Get HLS stream
            string hlsUrl = jo["HLSURL"].ToString();
            video = new ungap.CloudBox.Video(hlsUrl, 0, "video/html5", "brightcove:" + video_id, dtStarttime, 0, true);
            smil.videos.Add(video);

            // Get SMIL text
            string strSmil = smil.toXml();
            // Set MIME type for returned data
            // http://stackoverflow.com/questions/992533/wcf-responseformat-for-webget
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/smil";
            // Return as a stream so that data not gobbled up by the MS serializer
            return new System.IO.MemoryStream(ASCIIEncoding.Default.GetBytes(strSmil));
        }

        /* {
            "id": 2624888559001,
            "customFields": {
                "cb-landing-page": "http://malmostad.wallenmedia.se/kf20130321/",
                "cb-rec-starttime": "2013-03-21 12:45:31",
                "cb-rtmp-streamer": "rtmp://fl1.c04321.cdn.qbrick.com/04321",
                "cb-rtmp-files": "kf20130321_1.f4v",
                "cb-list": "1111111111111"
            },
            "FLVURL": "rtmp://brightcove.fcod.llnwd.net/a500/d17/&mp4:media/2494809924001/2494809924001_2632031678001_kf20130321.mp4&1377680400000&d40fc1a1309434382bbdea157dcde635",
            "HLSURL": "http://c.brightcove.com/services/mobile/streaming/index/master.m3u8?videoId=2624888559001&pubId=2494809924001"
        }
        */

        // ----------------------------------------------------------------------------------
        // TEST API
        // ----------------------------------------------------------------------------------

        public System.IO.Stream test(string video_id)
        {
            // Get video info from video ID
            string json = GetJson(video_id);

            // Set MIME type for returned data
            // http://stackoverflow.com/questions/992533/wcf-responseformat-for-webget
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.ContentType = "application/json";
            // Return as a stream so that data not gobbled up by the MS serializer
            return new System.IO.MemoryStream(ASCIIEncoding.Default.GetBytes(json));
        }
    }
}
