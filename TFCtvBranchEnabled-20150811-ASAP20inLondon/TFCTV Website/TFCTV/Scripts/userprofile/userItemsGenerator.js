var featureItem = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        var no_of_friends = 0;
        $.each(params.json_str, function (x, y) {
            if (ctr == 1)
                html += '<ul class="itemListing">';
            html += '<li>';
            if (params.type == 'show') {
                html += '<div class="imgShowThumbBg_1">';
                html += '<a href="/Show/Details/' + y.ShowId + '"><img src="' + y.ShowImageUrl + '" style="width:65px;height:auto" title="' + y.ShowName + '"/></a>';
                html += '</div>';
            }
            else if (params.type == 'person') {
                html += '<div class="imgShowThumbBg_1">';
                if (y.ShowImageUrl != null)
                    html += '<a href ="/Celebrity/Profile/' + y.ShowId + '"><img src="' + y.ShowImageUrl + '" style="width:65px;height:auto" title ="' + y.CelebrityFullName + '"/></a>';
                else
                    html += '<a href ="/Celebrity/Profile/' + y.ShowId + '"><img src="http://cdnassets.tfc.tv/content/images/celebrity/profilephoto.png" witdh= "65" height="65" title ="' + y.CelebrityFullName + '"/></a>';
                html += '</div>';
            }

            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
            }
            ctr++;
        });

        var json_length = params.json_str.length;

        if (params.type == 'profile') {
            if ("friends" in params.json_str && params.json_str.friends.length > 0) {
                var liHtml = '';
                ctr = 1;
                for (var i = 0; i < params.json_str.friends.length; i++) {
                    var currFriend = params.json_str.friends[i];
                    if (currFriend["isSiteUID"] == true) {
                        if (ctr == 1)
                            liHtml += '<ul class="itemListing">';
                        liHtml += '<li>';
                        liHtml += '<div class="imgFriendsThumbBg_1">';
                        liHtml += '<a href="/Profile/' + currFriend["UID"] + '"><img src="' + currFriend['thumbnailURL'] + '" width="45" height="45" alt = "' + currFriend['nickname'] + '" title = "' + currFriend['nickname'] + '"/></a>';
                        liHtml += '</div>';
                        liHtml += '</li>';
                        no_of_friends++;
                        if (ctr == params.itemperslide) {
                            liHtml += '</ul>'; ctr = 0; //reset counter
                        }
                        ctr++;
                    }
                }

                html = liHtml;
            }
            else
                html += "User has no friends in this site yet.";

            if (html.length == 0)
                html += "User has no friends in this site yet.";
            //html += '</ul>';
            json_length = no_of_friends;
        }

        nav_html += '<div class="breakStandard">';
        nav_html += '</div>';

        if (json_length > params.itemperslide) {
            nav_html += '<div class="navigation">';
            nav_html += '<a href="#" class="prev"></a>';
            nav_html += '<span id="navbtn"></span>';
            nav_html += '<a href="#" class="next"></a></div>';
        }

        nav_html += '<div class="clear breakStandard">';
        nav_html += '</div>';

        if (json_length > 0) {
            $('#' + params.container + ' .itemListBody .wrapper').html(html);
            if (json_length > params.itemperslide) {
                $('#' + params.container + ' .itemListBody').append(nav_html);
                $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollHorz', speed: 600,
                    timeout: 0, next: '#' + params.container + ' .itemListBody .navigation .next', prev: '#' + params.container + ' .itemListBody .navigation .prev'
                , pager: '#' + params.container + ' .itemListBody .navigation #navbtn'
                , pagerAnchorBuilder: function (idx, slide) {
                    return '<a href="#' + idx + '" class="bullet"></a>';
                }
                });
            }
            else {
                $('#' + params.container + ' .itemListBody').append('<div class="clear breakStandard"></div>');
            }
        }
        else
            $('#' + params.container + ' .itemListBody .wrapper').html(html);
    }
}