var featureItem = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize        

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="itemListing" style="z-index: 0;">';
            html += '<li>';

            if (params.type == 'video') {
                html += '<div class="imgShowThumbBg">';
                if (y.showImageUrl == null) {
                    html += '<img src="http://cdnassets.tfc.tv/content/images/celebrity/profilephoto.png" alt=""/>';
                }
                else {
                    html += '<img src="' + y.ShowImageUrl + '" alt=""/>';
                }
                html += '</div><div class="itemInfo">';
                html += '<span class="showTitle"><a href="/video/' + y.EpisodeId + '">' + y.ShowName + '</a></span><br />';
                html += '<span class="white">' + y.EpisodeDescription + '</span><br />';
                html += '<span class="white">Airdate: ' + y.EpisodeAirDate + '</span></div>';
            }
            else if (params.type == 'show' || params.type == 'person') {
                html += '<div class="imgShowThumbBg_1">';
                if (y.showImageUrl == null) {
                    html += '<img src="http://cdnassets.tfc.tv/content/images/celebrity/profilephoto.png" alt=""/>';
                }
                else {
                    html += '<img src="' + y.ShowImageUrl + '" alt=""/>';
                }
                html += '</div><div>';
                html += '<span class="showTitle"><a href="/Show/Details/' + y.ShowId + '">' + ((params.type == 'show') ? y.ShowName : y.CelebrityFullName) + '</a></span><br />';
                html += '</div>';

            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
            }
            ctr++;
        });
        if (ctr > 3) {
            nav_html += '<div class="breakStandard">';
            nav_html += '</div>';
            nav_html += '<div class="navigation">';
            nav_html += '<a href="#" class="prev">Prev</a>  |  <a href="#" class="next">Next</a></div>';
            nav_html += '<div class="breakStandard">';
            nav_html += '</div>';
        }
        $('#' + params.container + ' .itemListBody .wrapper').html(html);
        $('#' + params.container + ' .itemListBody').append(nav_html);
        $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 600,
            timeout: 0, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
        });
    }
}

var celebrityItem = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize        

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="itemListing">';
            html += '<li>';

            if (params.type == 'video') {
                html += '<div class="imgShowThumbBg">';
                if (y.showImageUrl == null) {
                    html += '<img src="http://cdnassets.tfc.tv/content/images/celebrity/profilephoto.png" alt=""/>';
                }
                else {
                    html += '<img src="' + y.ShowImageUrl + '" alt=""/>';
                }
                html += '</div><div class="itemInfo">';
                html += '<span class="showTitle"><a href="/video/' + y.EpisodeId + '">' + y.ShowName + '</a></span><br />';
                html += '<span class="white">' + y.EpisodeDescription + '</span><br />';
                html += '<span class="white">Airdate: ' + y.EpisodeAirDate + '</span></div>';
            }
            else if (params.type == 'show' || params.type == 'person') {
                html += '<div class="imgShowThumbBg_1">';
                if (y.showImageUrl == null) {
                    html += '<img src="http://cdnassets.tfc.tv/content/images/celebrity/profilephoto.png" alt=""/>';
                }
                else {
                    html += '<img src="' + y.ShowImageUrl + '" alt=""/>';
                }
                html += '</div><div>';
                html += '<span class="showTitle"><a href="/Celebrity/profile/' + y.ShowId + '">' + ((params.type == 'show') ? y.ShowName : y.CelebrityFullName) + '</a></span><br />';
                html += '</div><br />';
                /* html += '<span>';
                html += '<div id="social_component"><div id="componentDiv"></div>';
                html += '<script type="text/javascript">';
                html += 'var barID = "celebrity_' + y.ShowId + '";';
                html += 'var contentID = "' + y.ShowDescription + '";';
                html += 'celebrityLike(barID,contentID);';
                html += '</script>';
                html += '</div></span></div>'; ;
                */
            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
            }
            ctr++;
        });

        nav_html += '<div class="navigation">';
        nav_html += '<a href="#" class="prev"><img src = "/Content/images/celebrity/section_nav_arrow_left.png"/></a>    <a href="#" class="next"><img src = "/Content/images/celebrity/section_nav_arrow_right.png"/></a></div><br />';

        $('#' + params.container + ' .itemListBody .wrapper').html(html);
        $('#' + params.container + ' .itemListBody').append(nav_html);
        $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 600,
            timeout: 0, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
        });
    }
}