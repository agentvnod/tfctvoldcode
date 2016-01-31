var player_lastScrollTop = 0;
var player_offset = $('#playerContainer').offset();
var player_ht = $('#playerContainer').outerHeight();
player_offset = player_offset.top;
ptoSetFixed();
$(window).bind('scroll', function (e) {
    var st = $(this).scrollTop();
    if (st > player_lastScrollTop && player_lastScrollTop != 0) {
        if ($(window).scrollTop() > (player_offset + player_ht) && !$("#playerContainer").hasClass("tfc_tv") && !$("#pbtn-silver").is(":visible")) {            
            $('.video_part').css('padding-top', player_ht);
            $('#playerContainer').removeClass("regular_video").addClass('tfc_tv');
            $("#playerContainer > div").css("height","100%");
        }
    }
    else {
        if ($(window).scrollTop() <= player_offset) {
            $('#playerContainer').removeClass('tfc_tv').addClass("regular_video");            
            $('.video_part').css('padding-top', 0);
            $("#playerContainer > div").css("height", player_ht);
        }
    }
    player_lastScrollTop = st;
});

function ptoSetFixed() {
    if ($(window).scrollTop() > (player_offset + player_ht)) {
    }
    else {
        $('#playerContainer').removeClass('tfc_tv').addClass("regular_video");        
        $('.video_part').css('padding-top', 0);
        $("#playerContainer > div").css("height", player_ht);
    }
}


