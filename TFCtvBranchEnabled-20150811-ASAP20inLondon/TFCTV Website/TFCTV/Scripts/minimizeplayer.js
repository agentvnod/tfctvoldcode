var player_lastScrollTop = 0;
var player_offset = $('#playerContainer').offset();
var player_ht = $('#playerContainer').outerHeight();
player_offset = player_offset.top;
//console.log(player_offset + player_ht);
ptoSetFixed();
$(window).bind('scroll', function (e) {
    //console.log($(window).scrollTop());
    var st = $(this).scrollTop();
    if (st > player_lastScrollTop && player_lastScrollTop != 0) {
        if ($(window).scrollTop() > (player_offset + player_ht) && !$("#playerContainer").hasClass("tfc_tv") && !$("#pbtn-silver").is(":visible")) {
            //$('#playerContainer').fadeOut(200, function () {
            $('.video_part').css('padding-top', player_ht);
            $('#playerContainer').removeClass("regular_video").addClass('tfc_tv');
            setTimeout(function () { $f().getLogo().css({ top: 0, right: 5 }); }, 1);
            //    $('#playerContainer').fadeIn(200);
            //});
        }
    }
    else {
        if ($(window).scrollTop() <= player_offset) {
            $('#playerContainer').removeClass('tfc_tv').addClass("regular_video");
            setTimeout(function () { $f().getLogo().css({ top: 20, right: 20 }); }, 1);
            $('.video_part').css('padding-top', 0);
        }
    }
    player_lastScrollTop = st;
});

function ptoSetFixed() {
    if ($(window).scrollTop() > (player_offset + player_ht)) {
        //$('#playerContainer').removeClass("regular_video").addClass('tfc_tv');
        //setTimeout(function () { $f().getLogo().animate({ top: 0, right: 5 }); }, 500);
        //$('.video_part').css('padding-top', player_ht);
    }
    else {
        $('#playerContainer').removeClass('tfc_tv').addClass("regular_video");
        //setTimeout(function () { $f().getLogo().animate({ top: 20, right: 20 }); }, 500);
        $('.video_part').css('padding-top', 0);
    }
}

setTimeout(function () {
    $f().onFullscreen(function () {
        if ($('#playerContainer').hasClass("tfc_tv")) {
            $f().getPlugin("logo").css({ top: 20, right: 20 });
        }
    });

    $f().onFullscreenExit(function () {
        if ($('#playerContainer').hasClass("tfc_tv")) {
            $f().getPlugin("logo").css({ top: 0, right: 5 });
        }

    });
}, 3000);



