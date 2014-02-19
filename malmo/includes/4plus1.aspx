<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="4plus1.aspx.cs" Inherits="malmo.includes._4plus1" ViewStateMode="Disabled" %>

<!DOCTYPE html>

<html>
<head>
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <style>
        .container
        {
            width: 960px;
        }

        .video
        {
            float: left;
            height: 250px;
            background-color: #333;
        }

            .video.right
            {
                margin-left: 4px;
            }

            .video ul
            {
                margin: 0px;
                list-style: none;
                overflow: auto;
                background: #333;
                padding: 10px 0px;
            }

                .video ul li
                {
                    display: block;
                    float: left;
                    width: 160px;
                    height: 180px;
                    margin: 0px 10px;
                    position: relative;
                }

        li .video-time
        {
            position: absolute;
            right: 1px;
            top: 78px;
            background-color: #000;
            color: #ccc;
        }

        .video a:link
        {
            text-decoration: none;
        }

        .video li img
        {
            width: 160px;
            height: 100px;
        }

        .video li h3
        {
            font-style: normal;
            color: #fff;
            font-size: 12px !important;
            font-family: arial !important;
        }

        .video h1
        {
            background: #222;
            margin: 0px;
            color: #fff;
            padding: 5px 5px 5px 10px;
            font-size: 18px;
            font-family: verdana;
            line-height: 20px;
        }

        .video h2
        {
            background: #222;
            margin: 0px;
            color: #fff;
            font-size: 12px;
            font-family: verdana;
            padding: 5px 5px 5px 10px;
            text-align: center;
            line-height: 20px;
        }
    </style>
</head>
<body>
    <form id="startPageCarousel" runat="server">
        <div class="container">
            <div class="video" id="aktuelltContainer" runat="server">
            </div>
            <div class="video right" id="kfContainer" runat="server">
            </div>
        </div>
    </form>
</body>
</html>
