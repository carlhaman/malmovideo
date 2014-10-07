<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="clear.aspx.cs" Inherits="malmo.clear" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Cache control</title>
    <style>
        div#pageContent {
        max-width: 30em;
        margin: 0 auto;
        }
        .currentCache p a {
        float: right;
        }
        .currentCache p:hover {
        background-color: lightblue;
        }
        .deleted {
        border: 1px dotted red;
        background-color: lightgoldenrodyellow;
        padding: 0 .5em;
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="pageContent" runat="server">
    
    </div>
    </form>
</body>
</html>
