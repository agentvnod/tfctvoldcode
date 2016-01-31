
function generateCarouselsAppend(count) {
    var list = new featureItem({ count: count,
        type: 'thematicbundles', itemperslide: 1, container: 'featuredItems_featuredCelebrities', nid: 'nav-featuredcelebrities'
    }).listing();
}
var featureItem = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var json_length = params.count;

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
        $('#' + params.container + ' .itemListBody .navigroup').remove();
        if (json_length > params.itemperslide) {
            $('#' + params.container + ' .itemListBody').append(nav_html);
            $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 1000,
                timeout: 5000, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
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
        $('#' + params.container + ' .itemListBody').removeClass('hideElement');
    }
}