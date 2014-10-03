(function ($) {
    $(document).ready(function () {       
        setKfListItem();
        if (_bctid.length > 4) { loadRelatedVideos(); };
        if (_frontpage === true) { loadArchive(); };

        $('.videocarousel').slick({
            centerMode: true,
            variableWidth: true,
            centerPadding: '60px',
            slidesToShow: 3,
            slidesToScroll: 1,
            autoplay: true,
            autoplaySpeed: 5000,
            responsive: [
              {
                  breakpoint: 768,
                  settings: {
                      arrows: false,
                      centerMode: true,
                      centerPadding: '40px',
                      slidesToShow: 3
                  }
              },
              {
                  breakpoint: 480,
                  settings: {
                      arrows: false,
                      centerMode: true,
                      centerPadding: '40px',
                      slidesToShow: 1
                  }
              }
            ]
        });
    });
})(jQuery);

function setKfListItem() {
    var id = getUrlVars()["bctid"];
    var list = document.getElementById("kfList");
    if (list !== null) {
        list.value = id;
    }
}

function getUrlVars() {
    var vars = {};
    var parts = window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function (m, key, value) {
        vars[key] = value;
    });
    return vars;
}

function kfListChange() {
    var myselect = document.getElementById("kfList");
    /*
    alert(myselect.options[myselect.selectedIndex].value);
    */
    window.open("http://video.malmo.se?bctid=" + myselect.options[myselect.selectedIndex].value, "_self");
}

//Brightcove responsive

var player,
      APIModules,
      videoPlayer,
      experienceModule,
      percentage;

// utility
logit = function (context, message) {
    if (console) {
        console.log(context, message);
    }
};

function onTemplateLoad(experienceID) {
    logit("EVENT", "onTemplateLoad");
    player = brightcove.api.getExperience(experienceID);
    APIModules = brightcove.api.modules.APIModules;
}

function onTemplateReady() {
    logit("EVENT", "onTemplateReady");
    videoPlayer = player.getModule(APIModules.VIDEO_PLAYER);
    experienceModule = player.getModule(APIModules.EXPERIENCE);

    videoPlayer.getCurrentRendition(function (renditionDTO) {

        if (renditionDTO) {
            logit("condition", "renditionDTO found");
            calulateNewPercentage(renditionDTO.frameWidth, renditionDTO.frameHeight);
        } else {
            logit("condition", "renditionDTO NOT found");
            videoPlayer.addEventListener(brightcove.api.events.MediaEvent.PLAY, function (event) {
                calulateNewPercentage(event.media.renditions[0].frameWidth, event.media.renditions[0].frameHeight);
            });
        }
    });

    var evt = document.createEvent('UIEvents');
    evt.initUIEvent('resize', true, false, 0);
    window.dispatchEvent(evt);

    videoPlayer.play();
}

function calulateNewPercentage(width, height) {
    logit("function", "calulateNewPercentage");
    var newPercentage = ((height / width) * 100) + "%";
    logit("Video Width = ", width);
    logit("Video Height = ", height);
    logit("New Percentage = ", newPercentage);
    percentage = ((height / width) * 100);
}

window.onresize = function (evt) {
    var resizeWidth = $(".embed-container").width(),
        resizeHeight = (resizeWidth / 100) * percentage;

    if (experienceModule.experience.type === "html") {
        experienceModule.setSize(resizeWidth, resizeHeight);
    }
};

//Arkiv-kod
function archiveLoaded() {
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
}