﻿$(document).ready(function () {

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

    $("div#scrollbar").smoothDivScroll({
        autoScrollingMode: "",
        hotSpotScrolling: true,
        visibleHotSpotBackgrounds: "always",
        mousewheelScrolling: "horizontal",
        touchScrolling: true
    });

})(jQuery);