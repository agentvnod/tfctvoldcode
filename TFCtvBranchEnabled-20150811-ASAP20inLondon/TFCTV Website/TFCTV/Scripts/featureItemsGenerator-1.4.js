var featureItem = function (params) {
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
                html += '<a href="/Episode/Details/' + y.EpisodeId + '"><img src="' + y.EpisodeImageUrl + '" height="98" width="151" title="' + y.Blurb + '" alt=""/></a>';
                html += '</div><div class="itemInfo">';
                html += '<span class="showTitle"><a href="/Episode/Details/' + y.EpisodeId + '">' + y.ShowName + '</a></span><br />';
                html += '<span class="white">Date Aired: ' + y.EpisodeAirDate + '</span></div>';
            }
            else if (params.type == 'show') {
                html += '<div class="imgShowThumbBg_1">';
                html += '<a href="/Show/Details/' + y.ShowId + '"><img src="' + y.ShowImageUrl + '" width="178" height="246" /></a>';
                html += '</div><div class="itemInfo_1">';
                html += '<span class="showTitle"><a href="/Show/Details/' + y.ShowId + '">' + y.ShowName + '</a></span><br />';
                html += '<span class="white"></span></div>';
            }
            else if (params.type == 'person') {
                html += '<div class="imgShowThumbBg_1">';
                html += '<a href="/Celebrity/Profile/' + y.ShowId + '"><img src="' + y.ShowImageUrl + '" witdh= "178" height="246"/></a>';
                html += '</div><div>';
                html += '<span class="showTitle"><a href="/Celebrity/Profile/' + y.ShowId + '">' + y.CelebrityFullName + '</a></span><br />';
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
            $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 600,
                timeout: 0, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
                , pager: '#' + params.nid
                , pagerAnchorBuilder: function (idx, slide) {
                    return '<a href="#' + idx + '" class="bullet"></a>';
                }
            });
        }
        else {
            $('#' + params.container + ' .itemListBody').append('<div class="clear breakLarge navigroup"></div>');
        }
    }

    this.shorts = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctor = 1;

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="dsitemListing">';
            html += '<li>';
            if (params.type == 'video') {
                html += '<div class="chartItem"><div class="chartImage">';
                html += '<div class="chartImageHolder"><a href="/HaloHalo/' + y.EpisodeId + '"><img src="' + y.EpisodeImageUrl + '" height="98" width="151" title="' + y.Blurb + '" alt=""/></a></div>';
                html += '</div><div class="chartInfo">';
                html += '<div class="chartInfoTitle"><a href="/HaloHalo/' + y.EpisodeId + '">' + y.EpisodeName + '</a></div>';
                //html += '<div class="chartInfoDate">Posted on ' + y.EpisodeAirDate + '</div>';
                html += '<div class="chartInfoDetails">' + y.Blurb + '</div>';
                html += '</div><div class="clear"></div></div><div class="clear"></div>';
            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
                html += '<div class="clear" style="border-bottom: 1px solid #393939; width: 930px;"></div>';
            }
            ctr++;
            ctor++;
            if (ctor > params.maxCount)
                return false;
        });

        var json_length = params.json_str.length;

        var nonav = false;
        $('#' + params.container + ' .itemListBody .wrapper').html(html);

        if (json_length == 0)
            $('#' + params.container + ' .itemListBody .wrapper').html('<div class="breakStandard"></div><div class="white padLeftLarge">No videos available.</div>');

    }

    this.xmasshorts = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctor = 1;

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="dsitemListing">';
            html += '<li>';
            if (params.type == 'video') {
                html += '<div class="chartItem"><div class="chartImage">';
                html += '<div class="chartImageHolder"><a href="/KuwentongPasko/' + y.EpisodeId + '"><img src="' + y.EpisodeImageUrl + '" height="98" width="151" title="' + y.Blurb + '" alt=""/></a></div>';
                html += '</div><div class="chartInfo">';
                html += '<div class="chartInfoTitle"><a href="/KuwentongPasko/' + y.EpisodeId + '">' + y.EpisodeName + '</a></div>';
                //html += '<div class="chartInfoDate">Posted on ' + y.EpisodeAirDate + '</div>';
                html += '<div class="chartInfoDetails">' + y.Blurb + '</div>';
                html += '</div><div class="clear"></div></div><div class="clear"></div>';
            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
                html += '<div class="clear" style="border-bottom: 1px solid #393939; width: 930px;"></div>';
            }
            ctr++;
            ctor++;
            if (ctor > params.maxCount)
                return false;
        });

        var json_length = params.json_str.length;

        var nonav = false;
        $('#' + params.container + ' .itemListBody .wrapper').html(html);

        if (json_length == 0)
            $('#' + params.container + ' .itemListBody .wrapper').html('<div class="breakStandard"></div><div class="white padLeftLarge">No videos available.</div>');

    }

    this.dynamicshorts = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctor = 1;

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="dsitemListing">';
            html += '<li>';
            if (params.type == 'video') {
                html += '<div class="chartItem"><div class="chartImage">';
                html += '<div class="chartImageHolder"><a href="/' + params.section + '/' + y.EpisodeId + '"><img src="' + y.EpisodeImageUrl + '" height="98" width="151" title="' + y.Blurb + '" alt=""/></a></div>';
                html += '</div><div class="chartInfo">';
                html += '<div class="chartInfoTitle"><a href="/' + params.section + '/' + y.EpisodeId + '">' + y.EpisodeName + '</a></div>';
                //html += '<div class="chartInfoDate">Posted on ' + y.EpisodeAirDate + '</div>';
                html += '<div class="chartInfoDetails">' + y.Blurb + '</div>';
                html += '</div><div class="clear"></div></div><div class="clear"></div>';
            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
                html += '<div class="clear" style="border-bottom: 1px solid #393939; width: 930px;"></div>';
            }
            ctr++;
            ctor++;
            if (ctor > params.maxCount)
                return false;
        });

        var json_length = params.json_str.length;

        var nonav = false;
        $('#' + params.container + ' .itemListBody .wrapper').html(html);

        if (json_length == 0)
            $('#' + params.container + ' .itemListBody .wrapper').html('<div class="breakStandard"></div><div class="white padLeftLarge">No videos available.</div>');

    }

    this.dynamicshortsblank = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctor = 1;

        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="dsitemListing">';
            html += '<li>';
            if (params.type == 'video') {
                html += '<div class="chartItem"><div class="chartImage">';
                html += '<div class="chartImageHolder"><a target="_blank" href="/' + params.section + '/' + y.EpisodeId + '"><img src="' + y.EpisodeImageUrl + '" height="98" width="151" title="' + y.Blurb + '" alt=""/></a></div>';
                html += '</div><div class="chartInfo">';
                html += '<div class="chartInfoTitle"><a target="_blank" href="/' + params.section + '/' + y.EpisodeId + '">' + y.EpisodeName + '</a></div>';
                //html += '<div class="chartInfoDate">Posted on ' + y.EpisodeAirDate + '</div>';
                html += '<div class="chartInfoDetails">' + y.Blurb + '</div>';
                html += '</div><div class="clear"></div></div><div class="clear"></div>';
            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
                html += '<div class="clear" style="border-bottom: 1px solid #393939; width: 930px;"></div>';
            }
            ctr++;
            ctor++;
            if (ctor > params.maxCount)
                return false;
        });

        var json_length = params.json_str.length;

        var nonav = false;
        $('#' + params.container + ' .itemListBody .wrapper').html(html);

        if (json_length == 0)
            $('#' + params.container + ' .itemListBody .wrapper').html('<div class="breakStandard"></div><div class="white padLeftLarge">No videos available.</div>');
    }

    this.topUsers = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctor = 1;
        var isPromoAvailable = false;
        var ajsonLength = 0;

        $.each(params.json_str, function (x, y) {
            var photoURL = y.photoURL;
            if (ctr == 1)
                html += '<ul class="list-top-users">';
            html += '<li>';
            if (params.type == 'profile') {
                if (y.photoURL == null)
                    photoURL = params.defaultphotoURL;
                html += '<div class="tp-rank">#' + y.rank + '</div>';
                html += '<div><img src="' + photoURL + '" alt="' + y.name + '" title="' + y.name + '"/></div>';
                html += '<div class="tp-name">';
                html += '<span><a href="' + params.section + '/' + y.UID + '" class="tp-name-link">' + (y.type == 'self' ? 'You' : y.name) + '</a></span><br />';
                $.each(y.achievements, function (i, j) {
                    if (j.challengeID == params.promoName) {
                        html += '<span><span class="tp-points">' + j.pointsTotal + '</span> Entries</span></div>';
                        isPromoAvailable = true;
                        ajsonLength++;
                        return false;
                    }
                });

            }
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter 
            }
            ctr++;
            ctor++;
            if (ctor > params.maxCount)
                return false;
        });

        var json_length = params.json_str.length;
        var nonav = false;
        if (json_length == 0 || !isPromoAvailable)
            $('#' + params.container + ' .itemListBody .wrapper').html('<div class="clear"></div><div class="white padLeftLarge">No data available.</div>');
        else {
            $('#' + params.container + ' .itemListBody .wrapper').html(html);
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
                $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 600,
                    timeout: 0, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
                , pager: '#' + params.nid
                , pagerAnchorBuilder: function (idx, slide) {
                    return '<a href="#' + idx + '" class="bullet"></a>';
                }
                });
            }
            else {
                $('#' + params.container + ' .itemListBody').append('<div class="clear breakLarge navigroup"></div>');
            }
        }
    }

    this.topUsersV = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var ctor = 1;
        var isPromoAvailable = false;
        var ajsonLength = 0;

        $.each(params.json_str, function (x, y) {
            var photoURL = y.photoURL;
            if (ctr == 1)
                html += '<ul class="top-users-list-vertical">';
            html += '<li>';
            if (params.type == 'profile') {
                if (y.photoURL == null)
                    photoURL = params.defaultphotoURL;
                if (y.type == 'self')
                { params.rankContainer.text(y.rank); }
                html += '<div class="tp-rank-vertical">' + y.rank + '.</div>';
                html += '<div class="tp-img-vertical"><img src="' + photoURL + '" alt="' + y.name + '" title="' + y.name + '"/></div>';
                html += '<div class="white">';
                html += '<div><span><a href="' + params.section + '/' + y.UID + '" class="tp-name-link">' + (y.type == 'self' ? 'You' : y.name) + '</a></span></div>';
                $.each(y.achievements, function (i, j) {
                    if (j.challengeID == params.promoName) {
                        html += '<div>' + j.pointsTotal + ' Entries</div></div>';
                        isPromoAvailable = true;
                        ajsonLength++;
                        if (y.type == 'self')
                        { params.pointContainer.text(j.pointsTotal); }
                        return false;
                    }
                });
                html += '<div class="clear"></div>';
            }
            html += '</li>';
            ctr++;
            ctor++;
            if (ctor > params.maxCount)
                return false;
        });
        var json_length = params.json_str.length;
        var nonav = false;
        if (json_length == 0 || !isPromoAvailable)
            $('#' + params.container + ' .itemListBody .wrapper').html('<div class="clear"></div><div class="white padLeftLarge">No data available.</div>');
        else {
            $('#' + params.container + ' .itemListBody .wrapper').html(html);
        }
    }
}