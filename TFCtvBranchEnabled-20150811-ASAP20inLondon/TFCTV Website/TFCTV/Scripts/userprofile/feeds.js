feeds = {
    uf_timestamp: 0,
    au_timestamp: 0,
    ff_timestamp: 0
};

var defaultPhoto = 'http://cdnassets.tfc.tv/content/images/userprofile/photo.png';

function getUserCompleName(guid) {
    var friendsName = "";
    //GetUpdate SiteUserInfo
    jQuery.get("/User/GetSiteUserInfo", { id: guid }, function (response) {
        if (response.ErrorCode == 0) {
            friendsName = response.FirstName + " " + response.LastName;
        }
    });
    return friendsName;
}

function loadUserFeeds(params) {
    jQuery.get("/SocialEngagement/GetUserFeeds", { id: params.userId, timestamp: feeds.uf_timestamp, group: 'me', limit: 20 }, function (response) {
        var html = '';
        var userCompleName = $('#profileName').attr('userCompleName');
        if (response.errorCode == 0) {
            if (response.me.items.length > 0) {
                for (var i = 0; i < response.me.items.length; i++) {
                    if ("actorUID" in response.me.items[i].action && response.me.items[i].action.actorUID != null && response.me.items[i].action.actorUID.length > 0) {
                        html += '<div class="peopleContent">';
                        html += '<div class="people_thumbnail left"><a href="/Profile/' + response.me.items[i].action.actorUID + '"><img src="' + (response.me.items[i].sender.photoURL == undefined ? defaultPhoto : response.me.items[i].sender.photoURL) + '" width="45" title="' + userCompleName + '" alt="' + userCompleName + '"/></a></div>';
                        html += '<div class="peopleInfo">';

                        if ("action" in response.me.items[i] && "mediaItems" in response.me.items[i].action && response.me.items[i].action.mediaItems != null && response.me.items[i].action.mediaItems.length > 0 && "src" in response.me.items[i].action.mediaItems[0])
                            html += '<div class="people_thumbnail left"><a href="' + response.me.items[i].action.linkBack + '"><img src="' + response.me.items[i].action.mediaItems[0].src + '?width=56" alt="" width="56" /></a></div>';

                        html += '<div class="peopleName left"><a href="/Profile/' + response.me.items[i].action.actorUID + '">' + userCompleName + '</a> <span class="user-action">' + response.me.items[i].action.userMessage + '</span><span class="user-action"><i>' + response.me.items[i].action.description + '</i></span><br/>';
                        //html += '<div class="addFriendIcon left "><span class="user-action"><i>"' + response.me.items[i].action.description + '"</i></span></div><br/>';

                        html += '<div class="addFriend"><a href="' + response.me.items[i].action.linkBack + '">' + response.me.items[i].action.title + '</a></div></div>';
                        html += '</div>';

                        var date = new Date(response.me.items[i].timestamp);
                        html += '<div class="datetime right">' + date.toString().replace(/ GMT(.)*/g, "") + '</div>';
                        html += '<div class="separator clear"></div>'
                        html += '</div>';
                    }
                }

                if (feeds.uf_timestamp != 0) {
                    $("#userFeedsHolder").prepend("<div class='loadedFeed' style='display:none'>" + html + "</div>");
                    $(".loadedFeed").slideDown("slow").removeClass("loadedFeed");
                }
                else
                    $("#userFeedsHolder").html(html);

                feeds.uf_timestamp = new Date().getTime();
                $('#user-feeds-bar').removeClass("disable");
                $('#UserActivityFeeds').tinyscrollbar();
            }
            else {
                if (feeds.uf_timestamp == 0) {
                    $("#userFeedsHolder").html("<div>You have no activity in this site yet.</div>");
                }
            }
        }
        else
            feeds.uf_timestamp = 0;
    });
}

function loadEveryonesFeeds(params) {
    jQuery.get("/SocialEngagement/GetUserFeeds", { id: params.userId, timestamp: feeds.au_timestamp, group: 'everyone', limit: 20 }, function (response) {
        var html = ''; //01
        var everyonesName = "";
        if (response.errorCode == 0) {
            if (response.everyone.items.length > 0) {
                for (var i = 0; i < response.everyone.items.length; i++) {
                    if ("actorUID" in response.everyone.items[i].action && response.everyone.items[i].action.actorUID != null && response.everyone.items[i].action.actorUID.length > 0) {
                        if ("sender" in response.everyone.items[i]) {
                            everyonesName = getUserCompleName(response.everyone.items[i].action.actorUID);
                            everyonesName = everyonesName == "" ? response.everyone.items[i].sender.name : everyonesName;

                            html += '<div class="videosContent">'; //02
                            html += '<div class="people_thumbnail left">'; //03
                            html += '<a href="/Profile/' + response.everyone.items[i].action.actorUID + '"><img src="' + (response.everyone.items[i].sender.photoURL == undefined ? defaultPhoto : response.everyone.items[i].sender.photoURL) + '" width="45" title="' + everyonesName + '" alt="' + everyonesName + '"/></a>';
                            html += '</div>'; //03
                            html += '<div class="videosInfo">'; //04

                            if ("action" in response.everyone.items[i] && "mediaItems" in response.everyone.items[i].action && response.everyone.items[i].action.mediaItems != null && response.everyone.items[i].action.mediaItems.length > 0 && "src" in response.everyone.items[i].action.mediaItems[0]) {
                                html += '<div class="people_thumbnail left"><a href="' + response.everyone.items[i].action.linkBack + '"><img src="' + (response.everyone.items[i].action.mediaItems[0].src) + '?width=56" alt="" width="56" /></a></div>'; //05-05

                                //if ("action" in response.everyone.items[i] && "mediaItems" in response.everyone.items[i].action && response.everyone.items[i].action.mediaItems != null && response.everyone.items[i].action.mediaItems.length > 0 && "src" in response.everyone.items[i].action.mediaItems[0])
                                html += '<div class="videos-name left">'; //06
                            }
                            else
                                html += '<div class="videosName left">'; //06
                            var description = (response.everyone.items[i].action.description.length > 120) ? response.everyone.items[i].action.description.substring(0, 120) + '..."' : response.everyone.items[i].action.description;

                            html += '<a href="/Profile/' + response.everyone.items[i].action.actorUID + '">' + everyonesName + '</a> <span class="user-action">' + response.everyone.items[i].action.userMessage + '</span><span class="user-action"><i>' + description + '</i></span><br/>';
                            //html += '<div class="addFriendIcon left "><span class="user-action"><i>' + response.everyone.items[i].action.description + '</i></span></div><br/>';
                            html += '<div class="addFriend">'; //07

                            html += '<a href="' + response.everyone.items[i].action.linkBack + '">' + response.everyone.items[i].action.title + '</a>';
                            html += '</div>'; //07
                            html += '</div>'; //06
                            html += '</div>';

                            var date = new Date(response.everyone.items[i].timestamp);
                            html += '<div class="datetime right">' + date.toString().replace(/ GMT(.)*/g, "") + '</div>';
                            html += '<div class="separator clear"></div>';
                            html += '</div>'; //02
                        }
                    }
                }

                if (feeds.au_timestamp != 0) {
                    $("#everyonesFeedsHolder").prepend("<div class='loadedFeed' style='display:none'>" + html + "</div>");
                    $(".loadedFeed").slideDown("slow").removeClass("loadedFeed");
                }
                else
                    $("#everyonesFeedsHolder").html(html);
                feeds.au_timestamp = new Date().getTime();
                $('#all-user-feeds-bar').removeClass("disable");
                $('#EveryoneActivityFeeds').tinyscrollbar();
            }
            else {
                if (feeds.au_timestamp == 0) {
                    $("#everyonesFeedsHolder").html("<div>No activity in this site yet.</div>");
                }

            }
        }
        else
            feeds.au_timestamp = 0;
    });
}

function loadFriendsFeeds(params) {
    jQuery.get("/SocialEngagement/GetUserFeeds", { id: params.userId, timestamp: feeds.ff_timestamp, group: 'friends', limit: 20 }, function (response) {
        var html = '';
        var friendsName = "";
        var everyonesName = "";
        if (response.errorCode == 0) {
            if (response.friends.items.length > 0) {
                for (var i = 0; i < response.friends.items.length; i++) {
                    if ("actorUID" in response.friends.items[i].action && response.friends.items[i].action.actorUID != null && response.friends.items[i].action.actorUID.length > 0) {
                        friendsName = getUserCompleName(response.friends.items[i].action.actorUID);
                        friendsName = everyonesName == "" ? response.friends.items[i].sender.name : friendsName;


                        html += '<div class="peopleContent">';
                        html += '<div class="people_thumbnail left"><a href="/Profile/' + response.friends.items[i].action.actorUID + '"><img src="' + (response.friends.items[i].sender.photoURL == undefined ? defaultPhoto : response.friends.items[i].sender.photoURL) + '" width="45" title="' + friendsName + '" alt="' + friendsName + '"/></a></div>';
                        html += '<div class="peopleInfo">';

                        if ("action" in response.friends.items[i] && "mediaItems" in response.friends.items[i].action && response.friends.items[i].action.mediaItems != null && response.friends.items[i].action.mediaItems.length > 0 && "src" in response.friends.items[i].action.mediaItems[0])
                            html += '<div class="people_thumbnail left"><a href="' + response.friends.items[i].action.linkBack + '"><img src="' + response.friends.items[i].action.mediaItems[0].src + '?width=56" alt="" width="56" /></a></div>';

                        html += '<div class="peopleName left"><a href="/Profile/' + response.friends.items[i].action.actorUID + '">' + friendsName + '</a> <span class="user-action">' + response.friends.items[i].action.userMessage + '</span><span class="user-action"><i>' + response.friends.items[i].action.description + '</i></span><br/>';
                        //html += '<div class="addFriendIcon left "><span class="user-action"><i>' + response.friends.items[i].action.description + '</i></span></div><br/>';
                        html += '<div class="addFriend"><a href="' + response.friends.items[i].action.linkBack + '">' + response.friends.items[i].action.title + '</a></div></div>';
                        html += '</div>';

                        var date = new Date(response.friends.items[i].timestamp);
                        html += '<div class="datetime right">' + date.toString().replace(/ GMT(.)*/g, "") + '</div>';
                        html += '<div class="separator clear"></div>'
                        html += '</div>';
                    }
                }

                if (feeds.ff_timestamp != 0)
                    $("#friendsFeedsHolder").prepend(html);
                else
                    $("#friendsFeedsHolder").html(html);

                feeds.ff_timestamp = new Date().getTime();
                $('#friend-feeds-bar').removeClass("disable");
                $('#UserFriendsActivityFeeds').tinyscrollbar();
            }
            else {
                if (feeds.ff_timestamp == 0) {
                    $("#friendsFeedsHolder").html("<div>No friends's activity in this site yet.</div>");
                }
            }
        }
        else
            feeds.ff_timestamp = 0; //sets time to zero
    });
}
