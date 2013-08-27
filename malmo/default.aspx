<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="malmo.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta charset="utf-8" />
    <link href="http://www.malmo.se/assets-2.0/css/external-core.css" rel="stylesheet" type="text/css" media="all" />
    <link href="http://www.malmo.se/assets-2.0/jquery/malmo-theme.css" rel="stylesheet" type="text/css" media="all" />
    <link href="http://www.malmo.se/assets-2.0/css/malmo-print.css" rel="stylesheet" type="text/css" media="print" />
    <!--[if lt IE 7]><link href="http://www.malmo.se/assets-2.0/css/malmo-ie-css-fix.css" rel="stylesheet" type="text/css" media="all" /><![endif]-->
    <!--[if IE 7]><link href="http://www.malmo.se/assets-2.0/css/malmo-ie7-css-fix.css" rel="stylesheet" type="text/css" media="all" /><![endif]-->
    <link rel="shortcut icon" href="http://www.malmo.se/assets-2.0/img/malmo-favicon.ico" type="image/x-icon" />
    <script src="http://www.malmo.se/assets-2.0/jquery/jquery.js" type="text/javascript"></script>
    <script src="http://www.malmo.se/assets-2.0/js/malmo.js" type="text/javascript"></script>
    <script src="http://www.malmo.se/assets-2.0/js/external.js" type="text/javascript"></script>
    <script src="http://s7.addthis.com/js/250/addthis_widget.js" type="text/javascript"></script>

    <link href="css/playerCSS.css" rel="stylesheet" />
    <title></title>

</head>
<body>
    <div class="wrap-all">

        <!--Include the remote masthead here -->
        <div id="mastHead" runat="server"></div>

        <div class="main">
            <div class="columns-1">
                <div class="content-wrapper">
                    <div class="content-wrapper-1">
                        <div class="content-wrapper-2">
                            <div class="content-wrapper-3">
                                <div class="content-wrapper-4">
                                    <div class="content-wrapper-5">
                                        <div class="content-wrapper-6">
                                            <div class="pagecontent">

                                                <form id="form1" runat="server">
                                                    <div id="videoDetails" runat="server">
                                                    </div>
                                                    <div class="relatedVideos">Relaterade Videos</div>

                                                    <div class="playList" id="videoArchive" runat="server"></div>

                                                </form>

                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
    <script src="Scripts/jquery.lazyload.min.js?v=1.8.5" charset="utf-8"></script>
    <script type="text/javascript" charset="utf-8">
        $(function () {
            $("img.lazy").lazyload({
                effect: "fadeIn"
                /*
                appear : function(elements_left, settings) {
                    console.log("appear");
                    console.log(elements_left);
                    //console.log(this, elements_left, settings);
                },
                load : function(elements_left, settings) {
                    console.log("load");
                    console.log(elements_left);
                    //console.log(this, elements_left, settings);
                }
                */
            });
        });
    </script>
</body>
</html>
