(function ($) {
    $(document).ready(function () {

        $('#searchButton').click(function () {
            /*
            $('.tooltip').tooltipster('destroy');
            */
        });


        $('.accordion > dt > h2').click(function () {
            $('.accordion > dt > h2').removeClass("active");
            $(this).addClass("active");
            $('#archiveContent').html($(this).parent().next().html());
            $('#archiveContent').find('img.lazy').lazyload({ skip_invisible: false });
            /*
            $('#archiveContent').find('.tooltip').tooltipster('destroy');
            $('#archiveContent').find('.tooltip').tooltipster({
                theme: '.tooltipster-shadow',
                delay: 100,
                maxWidth: 420,
                touchDevices: false
            });
            */
            $('.archiveBlock').height($('.archiveVideos').height() + 25);

            return false;
        });

        $('.accordion > dt > h2').first().click();

        /**/
        
        setKfListItem();

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
    var parts = window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function(m,key,value) {
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