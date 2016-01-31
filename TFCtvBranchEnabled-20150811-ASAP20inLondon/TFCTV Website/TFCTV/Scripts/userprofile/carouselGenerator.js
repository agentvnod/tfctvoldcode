var carousel = function (params) {
    this.listing = function () {
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctr = params.slides;

        $.each(params.json_str, function (x, y) {
            html += '<div id="slide-' + y.CarouselSlideId + '" class="slides">';
            html += '<a href="#">';
            html += '<img width="940" height="300" src="' + y.BannerImageUrl + '" />';
            html += '</a><div class="bannerInfo">';
            html += '<div class="bInfo">';
            html += '<span class="title"><a href="">' + y.Header + '</a></span>';
            html += '<p>' + y.Blurb + '</p>';
            html += '<div class="button"><a href="' + y.TargetUrl + '">' + y.ButtonLabel + '</a></div>';
            html += '</div></div></div>';

            return (x < ctr);
        });

        nav_html += '<a href="#" class="prev"><img src="http://cdnassets.tfc.tv/content/images/carousel/prev.png" width="28" height="42" alt="" /></a>';
        nav_html += '<a href="#" class="next"><img src="http://cdnassets.tfc.tv/content/images/carousel/next.png" width="28" height="42" alt="" /></a>';

        if (params.json_str.length > 0) {
            $('#' + params.container + ' .featureBannerItems .wrapper').html(html);
            $('#' + params.container + ' .navigation').html(nav_html);
            $('#' + params.container + ' .featureBannerItems .wrapper').cycle({ fx: 'scrollHorz', speed: 1000,
                timeout: 4000, next: '#' + params.container + ' .navigation .next', prev: '#' + params.container + ' .navigation .prev', pause: 1
            , before: slideUp, after: slideDown
            });
        }
    }
}

function slideDown() {
    var id = '#' + this.id;
    $(id + ' .bannerInfo').delay(300).slideDown('slow');
}

function slideUp() {
    var id = '#' + this.id;
    $(id + ' .bannerInfo').css('display', 'none');
}