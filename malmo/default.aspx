<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="malmo.index" EnableViewState="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta charset="utf-8" />

    <title runat="server" id="metaPageTitle"></title>

    <asp:Literal ID="fbMeta" runat="server"></asp:Literal>

    <asp:Literal ID="twMeta" runat="server"></asp:Literal>


</head>
<body runat="server" id="bodyTag">

    <form id="form1" runat="server">
        <div id="mastHead" runat="server"></div>
        <article class="body-copy" role="main">

            <div class="videoWrapper">
                <div class="wrapper">
                    <div id="error" runat="server"></div>
                    <div class="videoBlock gradient greyGradient">
                        <div id="videoDetails" runat="server"></div>
                    </div>
                </div>
            </div>
            <div class="descriptionWrapper">
                <div class="wrapper">
                    <div id="videoDescription" runat="server"></div>
                </div>
            </div>

            <div class="relatedWrapper">
                <div class="wrapper">
                    <div class="playlist" id="relatedVideos" runat="server"></div>
                </div>
            </div>

            <div class="archiveWrapper">
                <div class="wrapper">
                    <div id="videoSearch" class="playlist" runat="server">

                        <div class="searchField">
                            <input type="button" id="searchButton" class="gradient yellowGradient" onclick="search()" value="Sök" />
                            <input id="searchText" placeholder="Sök video" />
                        </div>
                        <h2>Videoarkiv</h2>

                        <div id="searchResultsDiv" class="searchResults" runat="server"></div>

                    </div>

                    <div class="archiveMenu" id="videoArchive" runat="server"></div>
                    <div class="archiveVideos">
                        <ul id="archiveContent" class="video_grid"></ul>
                    </div>
                </div>
            </div>
        </article>
    </form>


    <asp:Literal ID="scriptBlock" runat="server"></asp:Literal>
    <script type="text/javascript">
        function search() {
            var query = $('#searchText').val();
            if (query.length >= 1) {
                $.ajax({
                    url: "search.aspx?query=" + query,
                    dataType: "json"
                }).success(function (data) {
                    $('#searchResultsDiv').empty();
                    $('#searchResultsDiv').append("<h2>Sökresultat för " + query + "</h2>");
                    $.each(data, function (i, item) {
                        $('#searchResultsDiv').append(
                            "<div class=\"post\">"
                            + "<a href=\"?bctid=" + data[i].bctid + " \" class=\"videoBox\">"
                                + "<div class=\"item-video\">"
                                   + "<img src=\"" + data[i].imageURL + "\" alt=\"" + data[i].shortDescription + "\"/>"
                                + "</div>"
                                + "<h3>" + data[i].title + "</h3>"
                                + "<p>" + data[i].shortDescription + "</p>"
                                + "</a>"
                             + "</div>"
                            );
                    })
                }).complete(function () { });

            }
        }


        $('#searchText').keypress(function (e) {
            if (e.which == 13) {
                e.preventDefault();
                search();
            }
        })
    </script>
</body>
</html>
