<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="malmo.index" EnableViewState="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta charset="utf-8" />

    <title runat="server" id="metaPageTitle"></title>
    
    <asp:Literal ID="fbMeta" runat="server"></asp:Literal>

    <asp:Literal ID="twMeta" runat="server"></asp:Literal>

    <asp:Literal ID="scriptBlock" runat="server"></asp:Literal>
</head>
<body runat="server" id="bodyTag">
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
                            <div id="KFDisclaimer" runat="server" class="disclaimer"><h2>Har du frågor om webbsändningen?</h2>Kontakta Mikael Hellman, <a href="mailto:mikael.hellman@malmo.se">mikael.hellman@malmo.se</a>, 0734-32 32 19</div>
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
                </div>
            </div>

        </div>

    </div>

</body>
</html>
