var featureItem = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="itemListing">';
            html += '<li>';
            if (params.type == 'contender') {
                html += '<div class="imgShowThumbBg_1">';
                html += '<a href="/TFCkat/ContenderDetails/' + y.ShowId + '"><img src="' + y.ShowImageUrl + '" width= "178" height="246"/></a>';
                html += '</div><div class="carouseliteminfo">';
                html += '<span class="showTitle"><a href="/TFCkat/ContenderDetails/' + y.ShowId + '">' + y.CelebrityFullName + '</a></span><br />';
                html += '<span>' + y.EpisodeName + '</span><br />';
                html += '<span>' + y.ShowDescription + '</span><br />';
                html += '<div class="TFCKatLoves" rel="' + y.ShowId + '"></div><div class="TFCKatLovesNumber" rel="' + y.ShowId + '">' + y.EpisodeDescription + '</div><br />';
                html += '<span class="white"></span></div>';
            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
            }
            ctr++;
        });

        var json_length = params.json_str.length;

        var no_of_dots = Math.floor((json_length / params.itemperslide));

        nav_html += '<div class="breakStandard navigroup">';
        nav_html += '</div>';

        if (json_length > params.itemperslide) {
            nav_html += '<div class="navigation navigroup">';
            nav_html += '<a href="#" class="prev"></a>';
            nav_html += '<span id="' + params.nid + '"></span>';
            nav_html += '<a href="#" class="next"></a></div>';
        }

        nav_html += '<div class="clear breakStandard navigroup">';
        nav_html += '</div>';

        var nonav = false;
        $('#' + params.container + ' .itemListBody .wrapper').html(html);
        $('#' + params.container + ' .itemListBody .navigroup').remove();
        if (json_length > params.itemperslide) {
            $('#' + params.container + ' .itemListBody').append(nav_html);
            $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 1000,
                timeout: 3000, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
                , pager: '#' + params.nid
                , pagerAnchorBuilder: function (idx, slide) {
                    return '<a href="#' + idx + '" class="bullet"></a>';
                }
                , pause: 1,
                pauseOnPagerHover: 1,
                allowPagerClickBubble: false
               
            });
        }
        else {
            $('#' + params.container + ' .itemListBody').append('<div class="clear breakLarge navigroup"></div>');
        }
    }
}