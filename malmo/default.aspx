<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="malmo.index" EnableViewState="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta charset="utf-8" />

    <link href="http://www.malmo.se/assets-2.0/css/external-core.css" rel="stylesheet" type="text/css" media="all" />
    <link href="http://www.malmo.se/assets-2.0/jquery/malmo-theme.css" rel="stylesheet" type="text/css" media="all" />
    <link href="http://www.malmo.se/assets-2.0/css/malmo-print.css" rel="stylesheet" type="text/css" media="print" />
    <!--[if lt IE 7]><link href="http://www.malmo.se/assets-2.0/css/malmo-ie-css-fix.css" rel="stylesheet" type="text/css" media="all" /><![endif]-->
    <!--[if IE 7]><link href="http://www.malmo.se/assets-2.0/css/malmo-ie7-css-fix.css" rel="stylesheet" type="text/css" media="all" /><![endif]-->
    <!--[if gte IE 9]>
      <style type="text/css">
        .gradient {
           filter: none;
        }
      </style>
    <![endif]-->
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

    <!--[if IE]><link href="css/playerCSS_ie.css" type="text/css" rel="stylesheet"><![endif]-->

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

            $('#searchButton').click(function () {
                $('.tooltip').tooltipster('destroy');
            });

            $('.accordion > dt > h2').click(function () {
                $('.accordion > dt > h2').removeClass("active");
                $(this).addClass("active");
                $('#archiveContent').html($(this).parent().next().html());
                $('#archiveContent').find('img.lazy').lazyload({ effect: "fadeIn" });
                $('#archiveContent').find('.tooltip').tooltipster('destroy');
                $('#archiveContent').find('.tooltip').tooltipster({
                    theme: '.tooltipster-shadow',
                    delay: 100,
                    maxWidth: 420,
                    touchDevices: false
                });

                return false;
            });

            $('.accordion > dt > h2').first().click();

        })(jQuery);


    </script>
</head>
<body>
    <div class="wrap-all">

        <!--Include the remote masthead here -->
        <div id="mastHead" runat="server"></div>

        <div class="main">
            <div class="videoWrapper">
                <div class="pagecontent">

                    <form id="form1" runat="server">
                        <asp:ScriptManager ID="scriptManager" runat="server" />

                        <div class="videoBlock gradient greyGradient">
                            <div id="videoDetails" runat="server"></div>
                            <div class="playlist" id="relatedVideos" runat="server"></div>
                        </div>

                        <div class="archiveBlock">
                            <div id="videoSearch" class="playlist" runat="server">

                                <asp:UpdatePanel ID="searchResultsPanel" runat="server">
                                    <ContentTemplate>
                                        
                                        <div class="searchField">
                                            <asp:Button runat="server" ID="searchButton" class="gradient yellowGradient" OnClick="searchButton_Click" Text="Sök" />
                                            <asp:TextBox ID="searchText" Text="Sök video" runat="server"></asp:TextBox>
                                        </div>
                                        <h2>Videoarkiv</h2>
                                            
                                        <div id="searchResultsDiv" class="searchResults" runat="server" style="display: none;"></div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                            <div class="archiveMenu" id="videoArchive" runat="server"></div>
                            <div class="archiveVideos">
                                <ul id="archiveContent" class="video_grid"></ul>
                            </div>
                        </div>

                    </form>
                </div>
            </div>

        </div>

    </div>

</body>
</html>
