/* --------------------------------------------------------------------------
* Copyright: ungap AB
* Contact: anders.marten@ungap.com
* Created: 2012-02-15 - 2013-08-27
* --------------------------------------------------------------------------
* */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;


namespace ungap.CloudBox
{
    public class Video
    {
        private String _Src = "";
        public String Src
        {
            get { return _Src; }
            set { _Src = value; }
        }

        private int _Bitrate = 0;
        public int Bitrate
        {
            get { return _Bitrate; }
            set { _Bitrate = value; }
        }

        private String _Type = "video/";
        public String Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        private String _AgentData = "";
        public String AgentData
        {
            get { return _AgentData; }
            set { _AgentData = value; }
        }

        private DateTime _StartTime = DateTime.Now;
        public DateTime StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; }
        }

        private int _LiveVideoDelay = 0;   // seconds
        public int LiveVideoDelay
        {
            get { return _LiveVideoDelay; }
            set { _LiveVideoDelay = value; }
        }

        private Boolean _HasTimecode = false;   // indicates if the video stream has embedded timecode
        public Boolean HasTimecode
        {
            get { return _HasTimecode; }
            set { _HasTimecode = value; }
        }

        public Video(String src, int bitrate, String type, String agentData, DateTime startTime, int livevideodelay, Boolean hasTimecode)
        {
            _Src = src;
            _Bitrate = bitrate;
            _Type = type;
            _AgentData = agentData;
            _StartTime = startTime;
            _LiveVideoDelay = livevideodelay;
            _HasTimecode = hasTimecode;
        }
    }

    public class Videos : List<Video>
    {
        public XElement toXml(XElement xSwitch)
        {
            foreach (Video video in this)
            {
                xSwitch.Add(new XElement("video",
                                          new XAttribute("src", video.Src),
                                          new XAttribute("system-bitrate", video.Bitrate),
                                          new XAttribute("type", video.Type),
                                          new XAttribute("agent-data", video.AgentData),
                                          new XAttribute("has-timecode", video.HasTimecode.ToString()),
                                          new XAttribute("live-video-delay", video.LiveVideoDelay.ToString()),
                                          new XAttribute("start-time", video.StartTime.ToString("yyyy-MM-dd HH:mm:ss"))));
            }
            return xSwitch;
        }

        public class Smil
        {
            private String _Base = "";
            public String Base
            {
                get { return _Base; }
                set { _Base = value; }
            }

            private Videos _Videos = new Videos();
            public Videos videos
            {
                get { return _Videos; }
                set { _Videos = value; }
            }

            [NonSerialized]
            public String lastError = "";

            public String toXml()
            {
                try
                {
                    XElement xSmil = new XElement("smil",
                                                    new XElement("head",
                                                        new XElement("meta", new XAttribute("base", _Base))),
                                                    new XElement("body",
                                                        videos.toXml(new XElement("switch"))));

                    return xSmil.ToString();
                }
                catch (Exception ex)
                {
                }
                return "";
            }
        }

    }
}
