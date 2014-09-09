﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Caching;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace malmo
{
    public partial class index : System.Web.UI.Page
    {
        //private static string BCReadToken = "";
        private static string KFReadToken = System.Configuration.ConfigurationManager.AppSettings["KF_READ_URL_TOKEN"].ToString();
        private static string MReadToken = System.Configuration.ConfigurationManager.AppSettings["M_READ_URL_TOKEN"].ToString();
        private bool malmoKomin = false;
        private bool frontPage = false;
        private bool isFromKfAccount = false;

        string _bodyCssClasses = "";
        string _kfDropDownList = string.Empty;
        StringBuilder clientScripts = new StringBuilder();

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Build Page Indexes

            malmoKomin = isMalmoNetwork();
            //malmoKomin = true;

            string archiveString = "Archive";
            if (malmoKomin)
            {
                archiveString = "kominArchive";
            }
            videoArchive archive = (videoArchive)Cache[archiveString];
            if (archive == null)
            {
                buildVideoArchive builder = new malmo.buildVideoArchive();
                archive = builder.render(malmoKomin);
                //archive = buildVideoArchive(malmoKomin);
                Cache.Insert(archiveString, archive, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
            }

            _kfDropDownList = (string)Cache["kfListString"];
            if (_kfDropDownList == null)
            {
                _kfDropDownList = renderKFDropDownList(archive);
                Cache.Insert("kfListString", _kfDropDownList, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
            }


            #endregion

            #region Adapt page to Public/Internal viewers
            //För att inte indexera staging-server
            if (!Request.Url.Host.ToString().Contains("video.malmo.se"))
            {
                HtmlMeta robotMeta = new HtmlMeta();
                robotMeta.Name = "ROBOTS";
                robotMeta.Content = "NOINDEX, NOFOLLOW";
                Page.Header.Controls.Add(robotMeta);

                //för KOMIN
                _bodyCssClasses += "development ";
                //bodyTag.Attributes.Add("class", "development");



            }

            if (malmoKomin)
            {
                renderKominAssets();
                mastHead.InnerHtml = "<div class='app-title'><a href='/'>Webbvideo</a></div>";
            }
            if (!malmoKomin)
            {
                renderExternalAssets();
                //renderMasthead();
            }

            renderCommonAssets();
            #endregion

            string queryId = string.Empty;
            error.Visible = false;

            if (Request.QueryString["bctid"] != null)
            {
                queryId = Request.QueryString.GetValues("bctid").GetValue(0).ToString();
            }

            if (queryId.Length > 6)
            {
                getBrightcoveVideo(queryId, MReadToken);
            }

            else
            {
                if (Request.QueryString["public"] != null)
                {
                    if (Request.QueryString.GetValues("public").GetValue(0).ToString() == "no")
                    {
                        error.InnerHtml = "Du försöker spela en intern video från Malmö stad men kan inte verifieras som en behörig användare.";
                        error.Visible = true;
                    }
                }
                frontPage = true;
                getLatestVideo();
            }

            if (!isFromKfAccount)
            {
                renderVideoArchive(archive);

            }
            else { videoSearch.Visible = false; }

            addScriptsToPage();
        }
        private void renderKominAssets()
        {
            HtmlMeta viewport = new HtmlMeta();
            viewport.Name = "viewport";
            viewport.Content = "width=device-width, initial-scale=1.0";
            Page.Header.Controls.Add(viewport);
            HtmlMeta UAX = new HtmlMeta();
            UAX.Content = "IE=edge";
            UAX.HttpEquiv = "X-UA-Compatible";
            Page.Header.Controls.Add(UAX);

            HtmlLink kominCSS = new HtmlLink();
            kominCSS.Href = ResolveClientUrl("//assets.malmo.se/internal/3.0/malmo.css");
            kominCSS.Attributes["rel"] = "stylesheet";
            kominCSS.Attributes["type"] = "text/css";
            kominCSS.Attributes["media"] = "all";
            Page.Header.Controls.Add(kominCSS);

            HtmlLink favicon = new HtmlLink();
            favicon.Attributes["rel"] = "icon";
            favicon.Attributes["type"] = "image/x-icon";
            favicon.Href = ResolveClientUrl("//assets.malmo.se/internal/3.0/favicon.ico");
            Page.Header.Controls.Add(favicon);

            addStartupScripts("//assets.malmo.se/internal/3.0/malmo.js");
        }
        private void renderExternalAssets()
        {

            HtmlMeta viewPortMeta = new HtmlMeta();
            viewPortMeta.Name = "viewport";
            viewPortMeta.Content = "width=device-width, initial-scale=1.0";
            Page.Header.Controls.Add(viewPortMeta);

            //HtmlLink externalCore = new HtmlLink();
            //externalCore.Href = ResolveClientUrl("http://www.malmo.se/assets-2.0/css/external-core.css");
            //externalCore.Attributes["rel"] = "stylesheet";
            //externalCore.Attributes["type"] = "text/css";
            //externalCore.Attributes["media"] = "all";
            //Page.Header.Controls.Add(externalCore);

            Literal IEFix = new Literal();
            IEFix.Text = "<!--[if IE]><meta content='IE=edge' http-equiv='X-UA-Compatible'/><![endif]-->";
            Page.Header.Controls.Add(IEFix);

            Literal IE8PrintFix = new Literal();
            IE8PrintFix.Text = "<!--[if lte IE 8]><script src='//assets.malmo.se/external/v4/html5shiv-printshiv.js' type='text/javascript'></script><![endif]-->";
            Page.Header.Controls.Add(IE8PrintFix);

            HtmlLink malmoTheme = new HtmlLink();
            malmoTheme.Href = ResolveClientUrl("//assets.malmo.se/external/v4/malmo.css");
            malmoTheme.Attributes["rel"] = "stylesheet";
            malmoTheme.Attributes["type"] = "text/css";
            malmoTheme.Attributes["media"] = "all";
            Page.Header.Controls.Add(malmoTheme);

            //HtmlLink malmoPrint = new HtmlLink();
            //malmoPrint.Href = ResolveClientUrl("http://www.malmo.se/assets-2.0/css/malmo-print.css");
            //malmoPrint.Attributes["rel"] = "stylesheet";
            //malmoPrint.Attributes["type"] = "text/css";
            //malmoPrint.Attributes["media"] = "print";
            //Page.Header.Controls.Add(malmoPrint);



            Literal IE8Fix = new Literal();
            IE8Fix.Text = "<!--[if lte IE 8]><link href='//assets.malmo.se/external/v4/legacy/ie8.css' media='all' rel='stylesheet' type='text/css'/><![endif]-->";
            Page.Header.Controls.Add(IE8Fix);

            //HtmlGenericControl jquery = new HtmlGenericControl("script");
            //jquery.Attributes.Add("type", "text/javascript");
            //jquery.Attributes.Add("src", "http://www.malmo.se/assets-2.0/jquery/jquery.js");
            //Page.Header.Controls.Add(jquery);
            //addStartupScripts("http://www.malmo.se/assets-2.0/jquery/jquery.js");

            //HtmlGenericControl malmoJs = new HtmlGenericControl("script");
            //malmoJs.Attributes.Add("type", "text/javascript");
            //malmoJs.Attributes.Add("src", "http://www.malmo.se/assets-2.0/js/malmo.js");
            //Page.Header.Controls.Add(malmoJs);
            //addStartupScripts("http://www.malmo.se/assets-2.0/js/malmo.js");

            //HtmlGenericControl malmoExternal = new HtmlGenericControl("script");
            //malmoExternal.Attributes.Add("type", "text/javascript");
            //malmoExternal.Attributes.Add("src", "http://www.malmo.se/assets-2.0/js/external.js");
            //Page.Header.Controls.Add(malmoExternal);
            //addStartupScripts("http://www.malmo.se/assets-2.0/js/external.js");

            //HtmlGenericControl addThis = new HtmlGenericControl("script");
            //addThis.Attributes.Add("type", "text/javascript");
            //addThis.Attributes.Add("src", "http://s7.addthis.com/js/250/addthis_widget.js");
            //Page.Header.Controls.Add(addThis);
            //addStartupScripts("http://s7.addthis.com/js/250/addthis_widget.js");

            //HtmlGenericControl noConflict = new HtmlGenericControl("script");
            //noConflict.InnerText = "$.noConflict(true);";
            //Page.Header.Controls.Add(noConflict);

            //clientScripts.AppendLine("<script>$.noConflict(true);</script>");
            HtmlLink favicon = new HtmlLink();
            favicon.Attributes["rel"] = "shortcut icon";
            favicon.Attributes["type"] = "image/x-icon";
            favicon.Href = ResolveClientUrl("//assets.malmo.se/external/v4/favicon.ico");
            Page.Header.Controls.Add(favicon);

            addStartupScripts("//assets.malmo.se/external/v4/malmo.js");

            _bodyCssClasses += "mf-v4 ";
            //bodyTag.Attributes.Add("class", "mf-v4");

        }
        private void renderCommonAssets()
        {
            //HtmlGenericControl jquery = new HtmlGenericControl("script");
            //jquery.Attributes.Add("type", "text/javascript");
            //jquery.Attributes.Add("src", "http://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js");
            //Page.Header.Controls.Add(jquery);
            if (!malmoKomin)
            {
                // addStartupScripts("http://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js");
            }
            //HtmlGenericControl tooltipster = new HtmlGenericControl("script");
            //tooltipster.Attributes.Add("type", "text/javascript");
            //tooltipster.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/jquery.tooltipster.min.js");
            //Page.Header.Controls.Add(tooltipster);
            // addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/jquery.tooltipster.min.js");

            //HtmlGenericControl lazyload = new HtmlGenericControl("script");
            //lazyload.Attributes.Add("type", "text/javascript");
            //lazyload.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/jquery.lazyload.min.js");
            //Page.Header.Controls.Add(lazyload);
            addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/jquery.lazyload.min.js");

            //HtmlGenericControl ui = new HtmlGenericControl("script");
            //ui.Attributes.Add("type", "text/javascript");
            //ui.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery-ui-1.10.3.custom.min.js");
            //Page.Header.Controls.Add(ui);
            //addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery-ui-1.10.3.custom.min.js");

            //HtmlGenericControl kinetic = new HtmlGenericControl("script");
            //kinetic.Attributes.Add("type", "text/javascript");
            //kinetic.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery.kinetic.min.js");
            //Page.Header.Controls.Add(kinetic);
            //addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery.kinetic.min.js");

            //HtmlGenericControl mouseWheel = new HtmlGenericControl("script");
            //mouseWheel.Attributes.Add("type", "text/javascript");
            //mouseWheel.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery.mousewheel.min.js");
            //Page.Header.Controls.Add(mouseWheel);
            //addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery.mousewheel.min.js");

            //HtmlGenericControl smoothScroll = new HtmlGenericControl("script");
            //smoothScroll.Attributes.Add("type", "text/javascript");
            //smoothScroll.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery.smoothdivscroll-1.3-min.js");
            //Page.Header.Controls.Add(smoothScroll);
            //addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/smoothScroll/jquery.smoothdivscroll-1.3-min.js");

            HtmlLink tooltipsterCSS = new HtmlLink();
            tooltipsterCSS.Href = Request.Url.GetLeftPart(UriPartial.Authority) + "/css/tooltipster.min.css";
            tooltipsterCSS.Attributes["rel"] = "stylesheet";
            tooltipsterCSS.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(tooltipsterCSS);

            HtmlLink extensionsCSS = new HtmlLink();
            extensionsCSS.Href = Request.Url.GetLeftPart(UriPartial.Authority) + "/css/extensions.min.css";
            extensionsCSS.Attributes["rel"] = "stylesheet";
            extensionsCSS.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(extensionsCSS);

            //HtmlLink smoothDivScrollCSS = new HtmlLink();
            //smoothDivScrollCSS.Href = Request.Url.GetLeftPart(UriPartial.Authority) + "/css/smoothDivScroll.min.css";
            //smoothDivScrollCSS.Attributes["rel"] = "stylesheet";
            //smoothDivScrollCSS.Attributes["type"] = "text/css";
            //Page.Header.Controls.Add(smoothDivScrollCSS);

            HtmlLink playerCSS = new HtmlLink();
            playerCSS.Href = Request.Url.GetLeftPart(UriPartial.Authority) + "/css/player.min.css";
            playerCSS.Attributes["rel"] = "stylesheet";
            playerCSS.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(playerCSS);

            Literal IEplayerCSS = new Literal();
            IEplayerCSS.Text = "<!--[if IE]><link href=\"" + Request.Url.GetLeftPart(UriPartial.Authority) + "/css/player_ie.min.css\" type=\"text/css\" rel=\"stylesheet\"><![endif]-->";
            Page.Header.Controls.Add(IEplayerCSS);

            if (malmoKomin)
            {
                HtmlLink playerKominCSS = new HtmlLink();
                playerKominCSS.Href = Request.Url.GetLeftPart(UriPartial.Authority) + "/css/playerKomin.min.css";
                playerKominCSS.Attributes["rel"] = "stylesheet";
                playerKominCSS.Attributes["type"] = "text/css";
                Page.Header.Controls.Add(playerKominCSS);
            }

            //HtmlGenericControl startScript = new HtmlGenericControl("script");
            //startScript.Attributes.Add("type", "text/javascript");
            //startScript.Attributes.Add("src", Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/startupScript.js");
            //Page.Header.Controls.Add(startScript);
            addStartupScripts(Request.Url.GetLeftPart(UriPartial.Authority) + "/Scripts/startupScript.min.js");
            bodyTag.Attributes.Add("class", _bodyCssClasses);
        }
        private void addStartupScripts(string scriptSrc)
        {
            clientScripts.AppendLine("<script type='text/javascript' src='" + scriptSrc + "'></script>");
        }

        private void addScriptsToPage()
        {
            scriptBlock.Text = clientScripts.ToString();
        }

        private void setStandardMeta()
        {
            string title = "Videoarkiv &#x2013 Malmö stad";
            metaPageTitle.Text = title;
            string description = "Välkommen till Malmö stads videoarkiv! Här publicerar vi videoklipp av nyheter, händelser och evenemang som vi tror kan vara av intresse för dig som bor i, eller besöker Malmö.";
            string logoURL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Images/Logga.jpg";

            StringBuilder twitter = new StringBuilder();
            StringBuilder facebook = new StringBuilder();

            twitter.AppendLine("<meta name=\"twitter:title\" content=\"" + title + "\" />");
            facebook.AppendLine("<meta property=\"og:title\" content=\"" + title + "\"/>");
            twitter.AppendLine("<meta name=\"twitter:description\" content=\"" + description + "\" />");
            facebook.AppendLine("<meta property=\"og:description\" content=\"" + description + "\" />");
            twitter.AppendLine("<meta name=\"twitter:image:src\" content=\"" + logoURL + "\" />");
            facebook.AppendLine("<meta property=\"og:image\" content=\"" + logoURL + "\" />");
            facebook.AppendLine("<meta property=\"og:type\" content=\"website\"/>");
            twitter.AppendLine("<meta name=\"twitter:card\" content=\"summary\" />");
            twitter.AppendLine("<meta name=\"twitter:url\" content=\"http://video.malmo.se\" />");
            facebook.AppendLine("<meta property=\"og:url\" content=\"http://video.malmo.se\" />");

            fbMeta.Text = facebook.ToString();
            twMeta.Text = twitter.ToString();

        }

        private void renderMasthead()
        {

            string mastHeadString = (string)Cache["masthead"];
            if (mastHeadString == null)
            {
                var mastHeadRequest = (HttpWebRequest)HttpWebRequest.Create("http://www.malmo.se/assets-2.0/remote/external-masthead/");
                try
                {
                    var mastHeadResponse = mastHeadRequest.GetResponse();
                    Stream mastHeadStream;
                    mastHeadStream = mastHeadResponse.GetResponseStream();
                    using (StreamReader reader = new StreamReader(mastHeadStream))
                    {
                        mastHeadString = reader.ReadToEnd();
                        Cache.Insert("masthead", mastHeadString, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20));
                    }

                }
                catch (WebException ex) { mastHeadString = "<div>Error: " + ex.Message + "</div>\n"; }
            }
            mastHead.InnerHtml = mastHeadString;

        }

        private void parseBrightcoveVideo(string BCResponseString)
        {
            bool CBPlayer = false;
            bool kominVideo = false;

            JavaScriptSerializer js = new JavaScriptSerializer();

            var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);

            if (results.customFields != null)
            {
                var customFields = results.customFields;

                foreach (dynamic field in customFields)
                {
                    if (field.Name != null)
                    {
                        if (field.Name == "targetgroup")
                        {
                            if (field.Value == "Komin") { kominVideo = true; }
                        }
                        if (field.Name == "cblandingpage")
                        {
                            CBPlayer = true;
                        }
                    }
                }
            }

            if (kominVideo && !malmoKomin) { Response.Redirect("http://video.malmo.se?public=no", true); }

            if (BCResponseString != "null")
            {
                string metaHtml = string.Empty;
                VideoMeta meta = (VideoMeta)js.Deserialize(BCResponseString, typeof(VideoMeta));

                if (meta != null)
                {
                    string playerKey = string.Empty;
                    string playerId = string.Empty;

                    if (CBPlayer)
                    {
                        playerKey = System.Configuration.ConfigurationManager.AppSettings["KF_BC_PLAYERKEY"].ToString();
                        playerId = System.Configuration.ConfigurationManager.AppSettings["KF_BC_PLAYERID"].ToString();


                        metaHtml += "<div class=\"cb-container\">\n";
                        metaHtml += "<div class=\"embed-container\">\n";
                        metaHtml += "<iframe src=\"" + meta.customFields["cblandingpage"] + "\" frameborder=\"0\" allowTransparency=\"true\" allowfullscreen></iframe>\n";
                        metaHtml += "</div>\n";
                        metaHtml += "</div>\n";
                    }

                    if (!CBPlayer && !kominVideo || !CBPlayer && kominVideo && malmoKomin)
                    {
                        //playerKey = System.Configuration.ConfigurationManager.AppSettings["M_BC_PLAYERKEY"].ToString();
                        //playerId = System.Configuration.ConfigurationManager.AppSettings["M_BC_PLAYERID"].ToString();
                        //745456405001 - Chromeless ID
                        //AQ~~,AAAArZCmTQE~,w5iz83926fkXk5wAB6K2HNZ2NUmtlRla - Chromeless Key


                        //För stats på olika användare
                        if (kominVideo)
                        {
                            playerId = "2810881921001";
                            playerKey = "AQ~~,AAAArZCmTQE~,w5iz83926fm7oUsR94lRtjiPJ3VicQiA";
                        }
                        else
                        {
                            playerId = "2810881920001";
                            playerKey = "AQ~~,AAAArZCmTQE~,w5iz83926flwNgeVE8x1_ZgoF5t7oTGp";
                        }

                        metaHtml += "<div class=\"bc-container\">\n";
                        metaHtml += "<div class=\"embed-container\">\n";
                        //metaHtml += "<iframe src=\"http://link.brightcove.com/services/player/bcpid745456405001?bckey=AQ~~,AAAArZCmTQE~,w5iz83926fkXk5wAB6K2HNZ2NUmtlRla&bctid=" + meta.id.ToString() + "\" frameborder=\"0\" allowfullscreen></iframe>\n";
                        //testar med javascript istället
                        metaHtml += @"

                                    <script language='JavaScript' type='text/javascript' src='http://admin.brightcove.com/js/BrightcoveExperiences.js'></script>

                                    <object id='myExperience" + meta.id.ToString() + @"' class='BrightcoveExperience'>
                                      <param name='bgcolor' value='#808080' />
                                      <param name='width' value='480' />
                                      <param name='height' value='270' />
                                      <param name='playerID' value='" + playerId + @"' />
                                      <param name='playerKey' value='" + playerKey + @"' />
                                      <param name='isVid' value='true' />
                                      <param name='isUI' value='true' />
                                      <param name='dynamicStreaming' value='true' />
                                      <param name='linkBaseURL' value='http://video.malmo.se?bctid=" + meta.id.ToString() + @"' />
                                      <param name='wmode' value='opaque' />
                                      <param name='htmlFallback' value='true' />
                                      <param name='@videoPlayer' value='" + meta.id.ToString() + @"' />
                                    </object>
                                    <script type='text/javascript'>brightcove.createExperiences();</script>

                                    <!-- End of Brightcove Player -->
                                    ";


                        metaHtml += "</div>\n";
                        metaHtml += "</div>\n";
                    }
                    if (CBPlayer)
                    {
                        metaHtml += "<div class=\"descriptionBox\">\n";                       
                    }
                    else
                    {
                        metaHtml += "<div class=\"descriptionBoxBC\">\n";
                    }
                    metaHtml += "<div class=\"videoDescription\">\n";

                    StringBuilder faceBookMeta = new StringBuilder();
                    StringBuilder twitterMeta = new StringBuilder();

                    if (meta.id > 0)
                    {
                        twitterMeta.AppendLine("<meta name=\"twitter:card\" content=\"player\" />");
                        twitterMeta.AppendLine("<meta name=\"twitter:url\" content=\"http://video.malmo.se/?bctid=" + meta.id.ToString() + "\" />");

                        string twitterPlayerUrl = "https://link.brightcove.com/services/player/bcpid" + playerId + "?bckey=" + playerKey + "&bctid=" + meta.id.ToString() + "&secureConnections=true&autoStart=false&height=100%25&width=100%25";

                        twitterMeta.AppendLine("<meta name=\"twitter:player\" content=\"" + twitterPlayerUrl + "\" />");
                        twitterMeta.AppendLine("<meta name=\"twitter:player:width\" content=\"360\" />");
                        twitterMeta.AppendLine("<meta name=\"twitter:player:height\" content=\"200\" />");

                        faceBookMeta.AppendLine("<meta property=\"og:url\"  content=\"http://video.malmo.se/?bctid=" + meta.id.ToString() + "\"/>");
                    }
                    if (meta.name != null)
                    {
                        metaHtml += "<h1>" + meta.name + "</h1>\n";

                        metaPageTitle.Text = meta.name + " &#x2013 Malmö stad";

                        twitterMeta.AppendLine("<meta name=\"twitter:title\" content=\"" + meta.name + "\" />");
                        faceBookMeta.AppendLine("<meta property=\"og:title\" content=\"" + meta.name + "\"/>");
                    }
                    if (meta.shortDescription != null)
                    {
                        metaHtml += "<p>" + meta.shortDescription + "</p>\n";

                        twitterMeta.AppendLine("<meta name=\"twitter:description\" content=\"" + meta.shortDescription + "\" />");
                        faceBookMeta.AppendLine("<meta property=\"og:description\" content=\"" + meta.shortDescription + "\" />");
                    }
                    if (meta.videoStillURL != null)
                    {

                        twitterMeta.AppendLine("<meta name=\"twitter:image:src\" content=\"" + meta.videoStillURL + "\" />");
                        faceBookMeta.AppendLine("<meta property=\"og:image\" content=\"" + meta.videoStillURL + "\" />");
                    }
                    metaHtml += "<div class=\"extraMeta\">\n";
                    if (meta.length > 0) { metaHtml += "Längd: " + new TimeSpan(0, 0, 0, 0, (int)meta.length).ToString(@"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture) + "<br/>"; }
                    if (meta.publishedDate != null)
                    {
                        DateTime UNIXepoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        long milli;
                        bool parse = long.TryParse(meta.publishedDate, out milli);
                        if (parse) { UNIXepoch = UNIXepoch.AddMilliseconds(milli); }
                        metaHtml += "Publicerad: " + UNIXepoch.ToShortDateString() + "<br/>\n";
                    }
                    if (meta.playsTotal != null) { metaHtml += "Visad: " + meta.playsTotal + "\n"; }
                    //if (meta.tags != null)
                    //{
                    //    metaHtml += "<ul class=\videoTags\">\n";
                    //    foreach (string s in meta.tags)
                    //    {
                    //        metaHtml += "<li>" + s + "</li>";
                    //    }
                    //    metaHtml += "</ul>\n";

                    //}
                    metaHtml += "</div>\n";
                    if (!kominVideo)
                    {
                        metaHtml += "<div class=\"social\">\n";
                        //Social sharing
                        //Facebook like
                        metaHtml += "<iframe src=\"//www.facebook.com/plugins/like.php?locale=sv_SE&amp;href=http%3A%2F%2Fvideo.malmo.se%2F%3Fbctid%3D" + meta.id.ToString() + "&amp;width=100&amp;height=21&amp;colorscheme=light&amp;layout=button_count&amp;action=like&amp;show_faces=false&amp;send=false\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:100px; height:21px;\" allowTransparency=\"true\"></iframe>\n";
                        //facebook share
                        //metaHtml += "<a href=\"#\" onclick=\"window.open('https://www.facebook.com/sharer/sharer.php?u=http%3A%2F%2Fvideo.malmo.se%2F%3Fbctid%3D" + meta.id.ToString() + "','facebook-share-dialog','width=626,height=436');return false;\" class=\"shareButton\">Dela på Facebook</a>\n";
                        //Twitter
                        metaHtml += "<a href=\"https://twitter.com/share\" class=\"twitter-share-button\"  data-url=\"http://video.malmo.se/?bctid=" + meta.id.ToString() + "\" data-lang=\"sv\">Tweeta</a><script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>\n";
                        metaHtml += "</div>\n";

                        string facebookPlayerId = "2915333897001";
                        faceBookMeta.AppendLine("<meta property=\"og:type\" content=\"movie\"/>");
                        faceBookMeta.AppendLine("<meta property=\"og:video:height\" content=\"270\"/>");
                        faceBookMeta.AppendLine("<meta property=\"og:video:width\" content=\"480\"/>");
                        faceBookMeta.AppendLine("<meta property=\"og:video\" content=\"http://c.brightcove.com/services/viewer/federated_f9/?isVid=1&isUI=1&playerID=" + facebookPlayerId + "&autoStart=true&videoId=" + meta.id.ToString() + "\">");
                        faceBookMeta.AppendLine("<meta property=\"og:video:secure_url\" content=\"https://secure.brightcove.com/services/viewer/federated_f9/?isVid=1&isUI=1&playerID=" + facebookPlayerId + "&autoStart=true&videoId=" + meta.id.ToString() + "&secureConnections=true\">");
                        faceBookMeta.AppendLine("<meta property=\"og:video:type\" content=\"application/x-shockwave-flash\">");

                        if (!frontPage)
                        {
                            fbMeta.Text = faceBookMeta.ToString();
                            twMeta.Text = twitterMeta.ToString();
                        }

                    }
                    if (frontPage || kominVideo)
                    {
                        setStandardMeta();
                    }
                    metaHtml += "</div>\n";

                    if (CBPlayer) {
                        metaHtml += "<div class=\"moreKF\"><h2>Fler kommunfullmäktigesändningar</h2>\n" + _kfDropDownList + "\n";
                        metaHtml += "<p>Läs mer om <a href=\"http://www.malmo.se/Kommun--politik/Politiker-och-beslut/Kommunfullmaktige.html\" target=\"_blank\">Kommunfullmäktige</a></p>\n";
                        metaHtml += "<h2>Har du frågor om webbsändningen?</h2><p>Kontakta Mikael Hellman, <a href=\"mailto:mikael.hellman@malmo.se\">mikael.hellman@malmo.se</a>, 0734-32 32 19</p></div>\n";
                    }
                    metaHtml += "</div>\n";
                    metaHtml += "<div style=\"clear: both;\"></div>\n";

                }


                videoDetails.InnerHtml = metaHtml;

            }
        }

        private string renderKFDropDownList(videoArchive archive)
        {
            StringBuilder html = new StringBuilder();
            videoCategory kfVideos = new videoCategory();
            if (archive != null)
            {
                foreach (videoCategory category in archive.categories)
                {
                    if (category.name == "Kommunfullmäktige")
                    {
                        kfVideos = category;
                    }
                }
            }
            System.Data.DataTable kfTable = new System.Data.DataTable("KF Videor");
            kfTable.Columns.Add("id", typeof(string));
            kfTable.Columns.Add("name", typeof(string));
            kfTable.Columns.Add("year", typeof(int));
            kfTable.Columns.Add("month", typeof(int));

            if (kfVideos != null)
            {
                foreach (videoItem video in kfVideos.videos)
                {
                    int month = 0;
                    int year = DateTime.Now.Year;
                    string id = "0";
                    string name = "KF Video";

                    if (video.id != null) { id = video.id; }
                    if (video.name != null) { name = video.name; }
                    if (video.tags != null)
                    {
                        foreach (string tag in video.tags)
                        {
                            //är det en månads-tagg
                            if (tag.ToUpper() == "JANUARI") { month = 1; }
                            if (tag.ToUpper() == "FEBRUARI") { month = 2; }
                            if (tag.ToUpper() == "MARS") { month = 3; }
                            if (tag.ToUpper() == "APRIL") { month = 4; }
                            if (tag.ToUpper() == "MAJ") { month = 5; }
                            if (tag.ToUpper() == "JUNI") { month = 6; }
                            if (tag.ToUpper() == "JULI") { month = 7; }
                            if (tag.ToUpper() == "AUGUSTI") { month = 8; }
                            if (tag.ToUpper() == "SEPTEMBER") { month = 9; }
                            if (tag.ToUpper() == "OKTOBER") { month = 10; }
                            if (tag.ToUpper() == "NOVEMBER") { month = 11; }
                            if (tag.ToUpper() == "DECEMBER") { month = 12; }

                            //eller en års-tag
                            string pattern = @"\d{4}";
                            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern);
                            if (r.IsMatch(tag))
                            {
                                year = Int32.Parse(tag);
                            }
                        }
                    }
                    kfTable.Rows.Add(id, name, year, month);
                }
            }
            //sortera sändningarna per år och månad
            System.Data.DataView dv = kfTable.DefaultView;
            dv.Sort = "year DESC, month DESC";

            //skapa en tabell över de unika åren 
            System.Data.DataTable yearsTable = dv.ToTable(true, "year");

            //bygg listan
            html.AppendLine("<select onChange=\"kfListChange()\" id=\"kfList\">");
            foreach (System.Data.DataRow r in yearsTable.Rows)
            {
                html.AppendLine("\t<optgroup label=\"" + r["year"].ToString() + "\">");
                foreach (System.Data.DataRow kfRow in kfTable.Rows)
                {
                    if (kfRow["year"].ToString() == r["year"].ToString())
                    {

                        html.AppendLine("\t\t<option value=\"" + kfRow["id"].ToString() + "\">" + kfRow["name"].ToString().Replace(kfRow["year"].ToString(), "") + "</option>");
                    }
                }
                html.AppendLine("\t</optgroup>");
            }
            html.AppendLine("</select>");

            return html.ToString();
        }

        private void getBrightcoveVideo(string brightcoveId, string token)
        {
            if (token == KFReadToken) { isFromKfAccount = true; }

            Stream dataStream;
            string requestFields = "id,name,shortDescription,publishedDate,tags,customFields,videoStillURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_video_by_id&video_id={0}&video_fields={1}&token={2}", brightcoveId, requestFields, token));
            request.Method = "POST";

            try
            {
                var response = request.GetResponse();
                dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();
                    if (BCResponseString != null)
                    {
                        if (BCResponseString != "null")
                        {
                            parseBrightcoveVideo(BCResponseString);

                            if (!isFromKfAccount)
                            {
                                getRelatedVideos(brightcoveId, token);
                            }
                        }
                        if (BCResponseString == "null" && token == MReadToken)
                        {
                            getBrightcoveVideo(brightcoveId, KFReadToken);
                        }
                    }

                }

            }
            catch (WebException ex) { }

        }

        private void setMetaOnPage(string name)
        {

        }

        private void getLatestVideo()
        {
            //Kf listan: 2623641282001
            //Aktuellt listan: 1172867907001
            bool isKf = false;
            if (Request.QueryString["kf"] != null)
            {
                isKf = true;
            }
            string token = MReadToken;
            string playlistId = "1172867907001";
            if (isKf) { token = KFReadToken; playlistId = "2623641282001"; }

            Stream dataStream;

            string videoFields = "id,name,shortDescription,publishedDate,tags,customFields,videoStillURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlist_by_id&playlist_id={0}&video_fields={1}&token={2}", playlistId, videoFields, token));
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
                        string id = results.videoIds[0];
                        if (id.Length > 6)
                        {
                            if (isKf) { Response.Redirect("?bctid=" + id.ToString()); }
                            getBrightcoveVideo(id, token);
                        }

                    }
                }

            }
            catch (WebException ex) { }


        }

        private void getRelatedVideos(string brightcoveId, string token)
        {
            Stream dataStream;
            string html = string.Empty;

            string videoFields = "id,name,shortDescription,customFields,videoStillURL,thumbnailURL,length,playsTotal";
            var request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_related_videos&video_id={0}&video_fields={1}&token={2}", brightcoveId, videoFields, token));
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
                        StringBuilder htmlBuilder = new StringBuilder();

                        htmlBuilder.AppendLine("<dl>\n");
                        htmlBuilder.AppendLine("<dt><h2>Relaterade Videor</h2></dt>\n");
                        //htmlBuilder.AppendLine("<div id=\"scrollbar\" class=\"related\">\n");
                        htmlBuilder.AppendLine("<ul class=\"related_videos_list\">\n");
                        //html += "<dl class=\"accordion\">\n";
                        //html += "<dt><h2>Relaterade Videos</h2></dt>\n";
                        //html += "<ul class=\"video_grid\" style=\"display:none;\">\n";
                        int counter = 0;
                        foreach (dynamic video in results.items)
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
                            if (counter <= 9)
                            {
                                if (!kominVideo || kominVideo && malmoKomin)
                                {
                                    string title = video.name.ToString().Replace("\"", "&quot");
                                    string description = video.shortDescription.ToString().Replace("\"", "&quot");

                                    //htmlBuilder.AppendLine("\t\t<li class=\"video_item tooltip\" title=\"<h2>" + title + "</h2><img src='" + video.videoStillURL.ToString() + "' width='400' height='225'/><p>" + description + "</p>\" >\n");
                                    htmlBuilder.AppendLine("\t\t<li class=\"video_item\">\n");
                                    //htmlBuilder.AppendLine("\t\t<div class=\"info-box\"><h2>" + title + "</h2><img src='" + video.videoStillURL.ToString() + "' width='400' height='225'/><p>" + description + "</p></div>\n");
                                    htmlBuilder.AppendLine("\t\t\t<a href=\"?bctid=" + video.id.ToString() + "\">\n");
                                    htmlBuilder.AppendLine("\t\t\t<div class=\"thumbnail\">\n");
                                    htmlBuilder.AppendLine("\t\t\t<img src=\"" + video.videoStillURL.ToString() + "\" width=\"160\" height=\"90\"/>\n");
                                    htmlBuilder.AppendLine("\t\t\t</div>\n");
                                    htmlBuilder.AppendLine("\t\t\t<div class=\"description\">\n");
                                    htmlBuilder.AppendLine("\t\t\t\t<h4>" + title + "</h4>\n");
                                    htmlBuilder.AppendLine("\t\t\t\t<p>" + description + "</p>\n");
                                    htmlBuilder.AppendLine("\t\t\t</div>\n");
                                    htmlBuilder.AppendLine("\t\t\t</a>");
                                    htmlBuilder.AppendLine("\t\t</li>\n");
                                    counter++;
                                }
                            }
                        }

                        htmlBuilder.AppendLine("\t<li style=\"clear:both;\"</li>\n");
                        htmlBuilder.AppendLine("</ul>\n");
                        //htmlBuilder.AppendLine("</div>\n");
                        htmlBuilder.AppendLine("</dl>\n");

                        html = htmlBuilder.ToString();

                    }
                }

            }
            catch (WebException ex) { }

            relatedVideos.InnerHtml = html;
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "lazy-loader", "<script type='text/javascript'>$('#relatedVideos').find('img.lazy').lazyload();</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "tooltip", "<script type='text/javascript'>$('#relatedVideos').find('.tooltip').tooltipster('destroy');$('#relatedVideos').find('.tooltip').tooltipster({theme: '.tooltipster-shadow',delay: 100,maxWidth: 420,touchDevices: false});</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "tooltip", "<script type='text/javascript'>$('#archiveContent').find('.tooltip').tooltipster({theme: '.tooltipster-shadow',delay: 100,maxWidth: 420,touchDevices: false});</script>", false);


        }

        private string searchBrightcoveVideos(string searchString)
        {
            string renderedResult = string.Empty;
            if (searchString == "") { renderedResult = "<p>Du måste ange ett sökord...</p>"; }
            else
            {
                renderedResult = "<p>Inga träffar på \"" + searchString + "\"...</p>";
                string videoFields = "id,name,shortDescription,videoStillURL,thumbnailURL,length,playsTotal,customFields";
                string[] searchWords = searchString.Split(' ');
                string bcSearchString = string.Empty;
                foreach (string word in searchWords)
                {
                    bcSearchString += "&any=" + word;
                }
                var mRequest = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=search_videos{0}&video_fields={1}&token={2}", bcSearchString, videoFields, MReadToken));
                mRequest.Method = "POST";
                try
                {
                    var response = mRequest.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    string BCResponseString = string.Empty;
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        BCResponseString = reader.ReadToEnd();

                        if (BCResponseString != null && BCResponseString != "null")
                        {
                            var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);
                            videoCategory cat = new videoCategory();
                            cat.name = "Search results";
                            cat.videos = new List<videoItem>();

                            foreach (dynamic video in results.items)
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
                                if (!kominVideo || kominVideo && malmoKomin)
                                {
                                    videoItem item = new videoItem();
                                    item.id = video.id;
                                    item.name = video.name.ToString().Replace("\"", "&quot");
                                    item.length = video.length;
                                    item.playsTotal = video.playsTotal;
                                    item.thumbnailURL = video.thumbnailURL;
                                    item.videoStillURL = video.videoStillURL;
                                    item.shortDescription = video.shortDescription.ToString().Replace("\"", "&quot");
                                    cat.videos.Add(item);
                                }
                            }
                            if (cat.videos.Count > 0) { renderedResult = renderSearchResults(cat); }
                        }
                    }
                }
                catch { }
            }
            return renderedResult;
        }

        private videoArchive buildVideoArchive(bool komin)
        {
            videoArchive archive = new videoArchive();
            archive.categories = new List<videoCategory>();
            string mArchivePlayerBcId = string.Empty;

            string kfPlaylistBcId = "2623641282001";

            if (komin) { mArchivePlayerBcId = "1213665896001"; }
            else { mArchivePlayerBcId = "1180742924001"; }

            string videoFields = "id,name,shortDescription,videoStillURL,thumbnailURL,length,playsTotal,tags,customFields";

            var mRequest = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlists_for_player_id&player_id={0}&video_fields={1}&token={2}", mArchivePlayerBcId, videoFields, MReadToken));
            var kfRequest = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://api.brightcove.com/services/library?command=find_playlist_by_id&playlist_id={0}&video_fields={1}&token={2}", kfPlaylistBcId, videoFields, KFReadToken));
            mRequest.Method = "POST";
            kfRequest.Method = "POST";

            //Get Malmö Account Items
            try
            {
                var response = mRequest.GetResponse();
                Stream dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();

                    if (BCResponseString != null && BCResponseString != "null")
                    {
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);

                        foreach (dynamic category in results.items)
                        {
                            videoCategory cat = new videoCategory();
                            cat.name = category.name;
                            cat.videos = new List<videoItem>();
                            foreach (dynamic video in category.videos)
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
                                    videoItem item = new videoItem();
                                    item.id = video.id;
                                    item.name = video.name.ToString().Replace("\"", "&quot");
                                    item.length = video.length;
                                    item.playsTotal = video.playsTotal;
                                    item.thumbnailURL = video.thumbnailURL;
                                    item.videoStillURL = video.videoStillURL;
                                    item.shortDescription = video.shortDescription.ToString().Replace("\"", "&quot");
                                    if (video.tags != null)
                                    {
                                        item.tags = new List<string>();
                                        var tags = video.tags;
                                        foreach (string tag in tags)
                                        {
                                            item.tags.Add((string)tag);
                                        }
                                    }

                                    cat.videos.Add(item);
                                }
                            }
                            archive.categories.Add(cat);
                        }




                    }
                }
            }
            catch { }
            ////Get KF Account Items
            try
            {
                var response = kfRequest.GetResponse();
                Stream dataStream = response.GetResponseStream();
                string BCResponseString = string.Empty;
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    BCResponseString = reader.ReadToEnd();

                    if (BCResponseString != null && BCResponseString != "null")
                    {
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BCResponseString);

                        videoCategory category = new videoCategory();
                        category.name = results.name;
                        category.videos = new List<videoItem>();

                        foreach (dynamic video in results.videos)
                        {
                            videoItem item = new videoItem();
                            item.id = video.id;
                            item.name = video.name.ToString().Replace("\"", "&quot");
                            item.length = video.length;
                            item.playsTotal = video.playsTotal;
                            item.thumbnailURL = video.thumbnailURL;
                            item.videoStillURL = video.videoStillURL;
                            item.shortDescription = video.shortDescription.ToString().Replace("\"", "&quot");
                            if (video.tags != null)
                            {
                                item.tags = new List<string>();
                                var tags = video.tags;
                                foreach (string tag in tags)
                                {
                                    item.tags.Add((string)tag);
                                }
                            }

                            category.videos.Add(item);
                        }
                        archive.categories.Add(category);
                    }
                }
            }
            catch { }
            return archive;

        }

        private void renderVideoArchive(videoArchive archive)
        {
            string archiveString = "archiveHtml";
            if (malmoKomin)
            {
                archiveString = "kominArchiveHtml";
            }
            string html = (string)Cache[archiveString];

            if (html == null)
            {
                StringBuilder htmlBuilder = new StringBuilder();
                int categoryId = 1;
                htmlBuilder.AppendLine("<dl class=\"accordion\">\n");
                foreach (videoCategory category in archive.categories)
                {
                    htmlBuilder.AppendLine("<dt><h2>" + category.name + "</h2></dt>");
                    htmlBuilder.AppendLine("<ul class=\"video_grid\" style=\"display:none;\">\n");
                    foreach (videoItem video in category.videos)
                    {
                        //htmlBuilder.AppendLine("<li class=\"video_item tooltip\" title=\"<h2>" + video.name + "</h2><img src='" + video.videoStillURL + "' width='400' height='225'/><p>" + video.shortDescription + "</p>\" >\n");
                        htmlBuilder.AppendLine("<li class=\"video_item\">\n");
                        htmlBuilder.AppendLine("<div class=\"info-box\"><h2>" + video.name + "</h2><img src=\"" + video.videoStillURL + "\" width=\"400\" height=\"225\"/><p>" + video.shortDescription + "</p></div>\n");
                        htmlBuilder.AppendLine("\t<a href=\"?bctid=" + video.id + "\">");
                        htmlBuilder.AppendLine("<img class=\"lazy\" src=\"Images/grey.gif\" data-original=\"" + video.thumbnailURL + "\" width=\"160\" height=\"90\"/>");
                        htmlBuilder.AppendLine("<h4>" + video.name + "</h4>");
                        htmlBuilder.AppendLine("</a>\n");
                        htmlBuilder.AppendLine("</li>\n");
                    }
                    htmlBuilder.AppendLine("<li style=\"clear:both;\"></li>\n");
                    htmlBuilder.AppendLine("</ul>\n");

                    categoryId++;
                }
                htmlBuilder.AppendLine("</dl>\n");
                html = htmlBuilder.ToString();
                //Cache.Insert("archiveHtml", html, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
                Cache.Insert(archiveString, html, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
            }
            videoArchive.InnerHtml = html;
        }

        private string renderSearchResults(videoCategory results)
        {

            StringBuilder htmlBuilder = new StringBuilder();
            //htmlBuilder.AppendLine("<ul class=\"video_grid\">\n");

            foreach (videoItem video in results.videos)
            {
                //htmlBuilder.AppendLine("<li class=\"video_item tooltip\" title=\"<h2>" + video.name + "</h2><img src='" + video.videoStillURL + "' width='400' height='225'/><p>" + video.shortDescription + "</p>\" >\n");
                htmlBuilder.AppendLine("<li class=\"video_item\">\n");
                htmlBuilder.AppendLine("<div class=\"info-box\"><h2>" + video.name + "</h2><img src=\"" + video.videoStillURL + "\" width=\"400\" height=\"225\"/><p>" + video.shortDescription + "</p></div>\n");
                htmlBuilder.AppendLine("\t<a href=\"?bctid=" + video.id + "\">");
                htmlBuilder.AppendLine("<img class=\"lazy\" src=\"Images/grey.gif\" data-original=\"" + video.thumbnailURL.ToString() + "\" width=\"160\" height=\"90\"/>");
                htmlBuilder.AppendLine("<h4>" + video.name + "</h4>");
                htmlBuilder.AppendLine("</a>\n");
                htmlBuilder.AppendLine("</li>\n");
            }
            htmlBuilder.AppendLine("<li style=\"clear:both;\"></li>\n");
            //htmlBuilder.AppendLine("</ul>\n");

            return htmlBuilder.ToString();

        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "slideUp", "<script type='text/javascript'>if ($('.searchResults').is(':visible')) {$('.searchResults').slideUp();}</script>", false);
            searchResultsDiv.InnerHtml = searchBrightcoveVideos(searchText.Text);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "tooltip", "<script type='text/javascript'>$('.tooltip').tooltipster({theme: '.tooltipster-shadow',delay: 100,maxWidth: 420,touchDevices: false});</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "lazy-load", "<script type='text/javascript'>$(\"img.lazy\").lazyload({ effect: \"fadeIn\" });</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "slideDown", "<script type='text/javascript'>$('.searchResults').slideDown();</script>", false);
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "populate", "<script type='text/javascript'>$('#archiveContent').html($('#searchResultsDiv').html());</script>", false);
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "lazy-load", "<script type='text/javascript'>$('#archiveContent').find('img.lazy').lazyload({ effect: \"fadeIn\" });</script>", false);
            //ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "tooltip", "<script type='text/javascript'>$('#archiveContent').find('.tooltip').tooltipster({theme: '.tooltipster-shadow',delay: 100,maxWidth: 420,touchDevices: false});</script>", false);
        }

        private bool isMalmoNetwork()
        {
            bool result = false;
            ipRangeCheck kominRange = new ipRangeCheck();
            var rangeList = new List<IpRange>();
            rangeList.Add(new IpRange(IPAddress.Parse("161.52.0.0"), IPAddress.Parse("161.52.255.255")));
            string adress = kominRange.GetIP4Address();
            result = kominRange.CheckIsIpInRange(adress, rangeList);
            return result;
        }
    }

    public class VideoMeta
    {
        public long id { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public string publishedDate { get; set; }
        public List<string> tags { get; set; }
        public string videoStillURL { get; set; }
        public int length { get; set; }
        public string playsTotal { get; set; }
        public Dictionary<string, string> customFields { get; set; }
    }
    public class Video
    {
        public long id { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public string videoStillURL { get; set; }
        public string thumbnailURL { get; set; }
        public int length { get; set; }
        public string playsTotal { get; set; }
    }
    public class Item
    {
        public long id { get; set; }
        public string referenceId { get; set; }
        public string name { get; set; }
        public string shortDescription { get; set; }
        public List<long> videoIds { get; set; }
        public List<Video> videos { get; set; }
        public string thumbnailURL { get; set; }
        public List<string> filterTags { get; set; }
        public string playlistType { get; set; }
    }
    public class RootObject
    {
        public List<Item> items { get; set; }
        public int page_number { get; set; }
        public int page_size { get; set; }
        public int total_count { get; set; }
    }



}