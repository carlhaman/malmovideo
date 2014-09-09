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

    <!--Include the remote masthead here -->
    <div id="mastHead" runat="server"></div>

    <div class="wrapper">
        <article class="body-copy" role="main">

            <form id="form1" runat="server">
                <asp:ScriptManager ID="scriptManager" runat="server" />
                <div id="error" runat="server"></div>
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
                                    <asp:TextBox ID="searchText" placeholder="Sök video" runat="server"></asp:TextBox>
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
        </article>
    </div>

    <asp:Literal ID="scriptBlock" runat="server"></asp:Literal>

</body>
</html>
