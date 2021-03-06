﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="render.aspx.cs" Inherits="malmo.render" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>

    <title></title>
    <style>
        * {
            margin: 0;
            padding: 0;
        }

        .va-videolist-container-outer {
            position: relative;
        }

        .va-videolist-container {
            max-width: 100%;
            min-width: 100%;
        }

        .not-expanded {
            white-space: nowrap;
            overflow: hidden;
        }

        .expanded {
            white-space: normal;
            overflow: visible;
        }

        button.slidearrow {
            height: 44px;
            width: 44px;
            opacity: 0.8;
            position: absolute;
            z-index: 20;
            margin-top: -21px;
            top: 76px;
            border: none;
        }

            button.slidearrow.is-right {
                right: 5px;
            }

        .is-invisible {
            display: none;
        }

        .va-videolist-container article {
            display: inline-block;
            vertical-align: top;
            width: 49%;
            position: relative;
        }

        .va-videolist-container article img {
             width: 100%;
        }

        .va-title {
            display: block;
            overflow: hidden;
            white-space: normal;
        }
        @media (min-width: 30em) {
            .va-videolist-container article {
                width: 30%;
            }
        }
                @media (min-width: 50em) {
            .va-videolist-container article {
                width: 20%;
            }
        }
    </style>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div id="responseContent" runat="server"></div>
        </div>
    </form>
    <script type="text/javascript">
        var isMobile = {
            Android: function () {
                return navigator.userAgent.match(/Android/i);
            },
            BlackBerry: function () {
                return navigator.userAgent.match(/BlackBerry/i);
            },
            iOS: function () {
                return navigator.userAgent.match(/iPhone|iPad|iPod/i);
            },
            Opera: function () {
                return navigator.userAgent.match(/Opera Mini/i);
            },
            Windows: function () {
                return navigator.userAgent.match(/IEMobile/i);
            },
            any: function () {
                return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
            }
        };

        if (isMobile.any()) {
            $(".slidearrow").css("display", "none");
            $(".va-videolist-container").css("overflow-x", "scroll");
        };

        $(".expand-button").click(function (event) {
            event.preventDefault();
            var videoList = $(this).parent().siblings(".va-videolist-container-outer").children(".va-videolist-container");
            var button = $(this).children(0);
            

            if (videoList.hasClass("not-expanded")) {
                videoList.parent().children(".is-right").removeClass("is-visible").addClass("is-invisible");
                if (videoList.parent().children(".is-left").hasClass("is-visible")) { videoList.parent().children(".is-left").removeClass("is-visible").addClass("is-invisible"); }

                videoList.fadeOut("fast", function () {
                    videoList.removeClass("not-expanded").addClass("expanded");
                    videoList.fadeIn("fast");
                    button.text("Dölj");
                });
            }
            if (videoList.hasClass("expanded")) {
                videoList.fadeOut("fast", function () {
                    videoList.removeClass("expanded").addClass("not-expanded");
                    videoList.fadeIn("fast", function () {
                        if (videoList.scrollLeft() > 0) {
                            videoList.parent().children(".is-left").removeClass("is-invisible").addClass("is-visible");
                        };
                        button.text("Visa alla");
                    });
                });
                videoList.parent().children(".is-right").removeClass("is-invisible").addClass("is-visible");

            }
        });

        $(".slidearrow").click(function (event) {
            event.preventDefault();
            if ($(this).hasClass("is-right")) {
                var container = $(this).siblings(".va-videolist-container");
                var videoWidth = container.children(0).width();
                var width = container.width() - videoWidth;
                container.animate({ scrollLeft: "+=" + width }, function () {
                    var offset = container.scrollLeft();
                    var leftButton = $(this).siblings(".is-left");
                    if (offset > 0) {
                        leftButton.removeClass("is-invisible").addClass("is-visible");
                    }
                });
            };
            if ($(this).hasClass("is-left")) {
                var container = $(this).siblings(".va-videolist-container");
                var videoWidth = container.children(0).width();
                var width = container.width() - videoWidth;
                container.animate({ scrollLeft: "-=" + width }, function () {
                    var offset = container.scrollLeft();
                    var leftButton = $(this).siblings(".is-left");
                    if (offset == 0) {
                        leftButton.removeClass("is-visible").addClass("is-invisible");
                    }
                });
            };
        });
    </script>
</body>
</html>
