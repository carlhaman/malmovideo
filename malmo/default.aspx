<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="malmo.index" EnableViewState="false"%>

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
    <script>$.noConflict(true);</script>

    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
    <script src="Scripts/jquery.tooltipster.min.js"></script>
    <script src="Scripts/jquery.lazyload.min.js"></script>
    <link href="css/tooltipster.css" rel="stylesheet" />
    <link href="css/playerCSS.css" rel="stylesheet" />

    <title runat="server" id="metaPageTitle"></title>

    <meta id="metaOgUrl" runat="server" property="og:url" />
    <meta id="metaOgTitle" runat="server" property="og:title" />
    <meta id="metaOgDescription" runat="server" property="og:description" />
    <meta id="metaOgImage" runat="server" property="og:image" />

    <meta name="twitter:card" content="player" />
    <meta id="metaTwitterUrl" name="twitter:url" runat="server" />
    <meta id="metaTwitterTitle" name="twitter:title" runat="server" />
    <meta id="metaTwitterDescription" name="twitter:description" runat="server" />
    <meta id="metaTwitterImage" name="twitter:image" runat="server" />
    <meta id="metaTwitterPlayer" name="twitter:player" runat="server" />
    

    <script>
        $(document).ready(function () {
            $('.tooltip').tooltipster({
                theme: '.tooltipster-shadow',
                delay: 100,
                maxWidth: 420,
                touchDevices: false
            });

            var allPanels = $('.accordion > ul').hide();

            $('.accordion > dt > h2').click(function () {
                if ($(this).parent().next().is(':visible')) {
                    $(this).removeClass("active");
                    allPanels.slideUp();
                }
                if ($(this).parent().next().is(':hidden')) {
                    allPanels.slideUp();
                    $('.accordion > dt > h2').removeClass("active");
                    $(this).parent().next().slideDown();
                    $(this).addClass("active");
                    $(this).parent().next().find('img.lazy').lazyload({ effect: "fadeIn" });
                }

                return false;
            });

        })(jQuery);


    </script>
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
                                                    <div id="videoDetails" runat="server"></div>
                                                    <div class="playlist" id="relatedVideos" runat="server"></div>
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

</body>
</html>
