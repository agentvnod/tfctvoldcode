/* File Created: February 2, 2012 */

var menu = function (params) {
    this.create = function () {
        var html = '';
        $.each(params.json_str, function () {
            html += '<div class="showList">';
            html += '<div class="categoryTitle floatLeft">';
            if (this.type == 1) {
                //html += '<a href="/Channel/List/' + this.id + '">' + this.name + '</a></div>';
                html += '<a href="/Live/Index/' + this.name.replace(' ', '') + '">' + this.name + '</a></div>';
            }
            else
                html += '<a href="/Category/List/' + this.id + '">' + this.name + '</a></div>';
            html += '<div class="showListItem_container floatLeft">';
            $.each(this.shows, function (x, y) {
                if (this.type == 1)
                    html += '<span class="showListItem"><a href="/Channel/Details/' + this.id + '">' + this.name + '</a></span>';
                else
                    html += '<span class="showListItem"><a href="/Show/Details/' + this.id + '">' + this.name + '</a></span>';
            });
            if (this.type == 1) {
                //html += '<span class="showListItem"><a href="/Channel/List/' + this.id + '" class="more">more...</a></span>';
            }
            else
                html += '<span class="showListItem"><a href="/Category/List/' + this.id + '" class="more">more...</a></span>';
            html += '</div><div class="clear"></div></div>';
        });

        $('#' + params.container + ' .subMenu').html(html);
    }
}
/* File Created: June 17 2013 */
var newmenu = function (params) {

    this.create = function () {
        var html = '<div class="subMenuLeaderboard"></div><ul class="revShowListContainer"><lh class="menuListHeader">GENRE CATEGORIES</lh>';

        $.each(params.json_str, function () {
            html += '<li id="' + this.name + '" class="revShowList">';
            html += '<p class="revCategoryTitle floatLeft">';
            if (this.type == 1) {
                //html += '<a href="/Channel/List/' + this.id + '">' + this.name + '</a></div>';
                html += '<a href="/Live/Index/' + this.name.replace(' ', '') + '">' + this.name + '</a></p>';
            }
            else
                html += '<a href="/Category/List/' + this.id + '">' + this.name + '</a></p>';

            html += '<div class="revShowListItem_containerDiv"><ul class="revShowListItem_container"><lh class="menuListHeader">FEATURED TITLES</lh>';
            $.each(this.shows, function (x, y) {
                if (this.type == 1) {

                    html += '<li class="abc"><span class="revShowListItem"><a href="/Channel/Details/' + this.id + '">' + this.name + '</a></span></li>';
                } else {

                    html += '<li class="abc"><span class="revShowListItem"><a href="/Category/List/' + this.id + '">' + this.name + '</a><div class="episodeContainer" id="c' + this.id + '"><div><span class="episodeMenuListHeader menuListHeader">LATEST EPISODES</span></div></div></li>';
                    getFeatureEpisodeDetails(this.id);
                }
            });

            if (this.type == 1) {
                //html += '<span class="showListItem"><a href="/Channel/List/' + this.id + '" class="more">more...</a></span>';
            }
            else
                html += '<span class="moreLink"><a href="/Category/List/' + this.id + '" class="more">More>></a></span>';
            html += '</li></ul></div>';
        });
        html += '</ul><div class="revCenterList"></div><div class="revEpisodeSamples"></div>';
        $('#' + params.container + ' #revSubMenu').html(html);



    }
}

function getFeatureEpisodeDetails(showid) {

    jQuery.get("/Menu/GetPreviewEpisodes/" + showid, function (list) {
        var pass = '';
        $.each(list, function (index) {
            pass += '<div class="menuEpisodeFeature"><a href="#"><img src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>' + this.Blurb + '</p><p><a href="#"><img src="../../../Content/images/menuBar/WatchNow.png"></a></p></div>';

        });
        pass += '<span class="moreEpisodesLink"><a href="/Show/Details/' + showid + '">More Episodes>></a></span>';
        var id = '#c' + showid;
        $(id).append(pass);

    });


}

var revmenu = function (params) {

    this.create = function () {
        var html = '<div id="dAllMenu"><ul class="ulAllMenu">';
        $.each(params.data, function (x, item) {
            html += '<li id="source' + this.container + '" class="liAllMenu rel=' + this.container.replace('menu','') + '"></li>';
            getGenreShowsEpisodes(this.url, this.container);


        });
        html += '</ul></div>';


        $('#invisibleMenuTree').html(html).promise().done(function () {
            hasLoaded = true;
        });
        //        if ($('#invisibleMenuTree').html().length == html.length)
        //         hasLoaded = true;


    }
    return true;
}
function getGenreShowsEpisodes(url, id) {
    var shtml = '<div class="dsourceGenre"><div class="menuListHeader"><h3>GENRE</h3></div><ul class="ulGenre">';
    jQuery.get(url, function (data) {

        $.each(data, function () {
            shtml += '<li id="' + this.name + '" class="liGenre">';
            //shtml += '<a href="/Category/List/' + this.id + '">' + this.name + '</a>';
            shtml += this.name;
            shtml += '<div class="dsourceShows"><div class="menuListHeader menuListheaderPixelbar"><h3>FEATURED SHOWS</h3></div><ul class="ulShows">';

            $.each(this.shows, function (x, y) {
                var showname = this.name;
                //shtml += '<li class="liShows"><a href="/Show/Details/' + this.id + '">' + this.name + '</a><div class="dLatestEpisodes"><div class="menuListHeader episodeHeader menuListheaderPixelbar">';
                shtml += '<li class="liShows">' + this.name + '<div class="dLatestEpisodes"><div class="menuListHeader episodeHeader menuListheaderPixelbar">';
                if (id == "menuEntertainment" || id == "menuNews")
                    shtml += '<h3>LATEST EPISODES</h3>';
                else if (id == "menuMovies")
                    shtml += '<h3>FEATURED MOVIE</h3>';
                else shtml += '<h3>LIVE STREAMING</h3>';
                shtml += '</div>';
                var epno = 0;
                $.each(this.features, function (x, y) {
                    if (x == 0) {
                        if (id == "menuEntertainment" || id == "menuNews") {

                            shtml += '<div class="dsourceEpisodes sourceEpLeft"><a href="/Episode/Details/' + this.EpisodeId + '"><img src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>';
                            shtml += this.Blurb;
                            shtml += '</p><p><a class="watchnow" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div>';
                        }
                        else {
                            shtml += '<div class="dsourceEpisodes dsourceMovieEpisode sourceEpLeft"><div class="posterImage"><a href="/Episode/Details/' + this.EpisodeId + '"><img src="' + this.ShowImageUrl + '"></a></div><div class="blurbWatchNow"><p><h4>' + showname + '</h4></p><p>';
                            shtml += this.Blurb;
                            shtml += '</p><p><a class="watchnow watchnowmovie" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div></div>';

                        }
                    } else {
                        if (id == "menuEntertainment" || id == "menuNews") {

                            shtml += '<div class="dsourceEpisodes"><a href="/Episode/Details/' + this.EpisodeId + '"><img src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>';
                            shtml += this.Blurb;
                            shtml += '</p><p><a class="watchnow" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div>';
                        }
                        else {
                            shtml += '<div class="dsourceEpisodes dsourceMovieEpisode"><div class="posterImage"><a href="/Episode/Details/' + this.EpisodeId + '"><img src="' + this.ShowImageUrl + '"></a></div><div class="blurbWatchNow"><p><h4>' + showname + '</h4></p><p>';
                            shtml += this.Blurb;
                            shtml += '</p><p><a class="watchnow watchnowmovie" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div></div>';

                        }
                    }
                   

                });
                if (id == "menuEntertainment" || id == "menuNews")
                    shtml += '<div class="moreLink"><a href="/Show/Details/' + this.id + '" class="more">All Episodes&raquo;</a></div>';
                shtml += '</div></li>';
            });

            shtml += '<div class="moreLink"><a href="/Category/List/' + this.id + '" class="more">All ' + this.name + ' Shows&raquo;</a></div>';
            shtml += '</ul></div></li>';
        });
        shtml += '</ul></div>';

        var id2 = "#source" + id;
        $(id2).html(shtml);

    });
}

