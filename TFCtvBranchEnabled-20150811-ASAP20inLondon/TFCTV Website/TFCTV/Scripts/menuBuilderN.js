var revmenu = function (params) {
    this.create = function () {
        var html = '<div id="dAllMenu"><ul class="ulAllMenu">';
        $.each(params.data, function (x, item) {
            html += '<li id="source' + this.container + '" class="liAllMenu" rel="' + this.container.replace('menu', '') + '"></li>';
            getGenreShowsEpisodes(this.url, this.container);
        });
        html += '</ul></div>';
        $('#invisibleMenuTree').html(html);
    }
    return true;
}

function getGenreShowsEpisodes(url, id) {
    var shtml = '<div class="dsourceGenre"><div class="menuListHeader"><h3>GENRE</h3></div><ul class="ulGenre">';
    jQuery.get(url, function (data) { //}).done(function (data) {
        $.each(data, function () {
            shtml += '<li id="' + this.name + '" class="liGenre">';
            shtml += this.name;
            shtml += '<div class="dsourceShows"><div class="menuListHeader menuListheaderPixelbar"><h3>FEATURED SHOWS</h3></div><ul class="ulShows">';
            $.each(this.shows, function (x, y) {
                var showname = this.name;
                shtml += '<li rel="' + this.id + '" class="liShows"><span>' + this.name + '</span><div id="' + this.id + '" class="dLatestEpisodes">';
                //                if (id == "menuEntertainment" || id == "menuNews")
                //                    shtml += '<h3>LATEST EPISODES</h3>';
                //                else if (id == "menuMovies")
                //                    shtml += '<h3>FEATURED MOVIE</h3>';
                //                else shtml += '<h3>LIVE STREAMING</h3>';
                //                shtml += '</div>';
                //				var featureLength = this.features.length;
                //                $.each(this.features, function (x, y) {
                //                    if (x == 0) {
                //                        if (featureLength>1) {
                //                            shtml += '<div class="dsourceEpisodes sourceEpLeft"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 151px; height: 98px;" src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>';
                //                            shtml += this.Blurb;
                //                            shtml += '</p><p><a class="watchnow" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div>';
                //                        }
                //                        else {
                //                            shtml += '<div class="dsourceEpisodes dsourceMovieEpisode sourceEpLeft"><div class="posterImage"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 186px; height: 257px;" src="' + this.ShowImageUrl + '"></a></div><div class="blurbWatchNow"><h4 style="width:200px;">' + showname + '</h4><p>';
                //                            shtml += this.Blurb;
                //                            shtml += '</p><p><a class="watchnow watchnowmovie" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div></div>';
                //                        }
                //                    } else {
                //                        if (featureLength>1) {
                //                            shtml += '<div class="dsourceEpisodes"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 151px; height: 98px;" src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>';
                //                            shtml += this.Blurb;
                //                            shtml += '</p><p><a class="watchnow" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div>';
                //                        }
                //                        else {
                //                            shtml += '<div class="dsourceEpisodes dsourceMovieEpisode"><div class="posterImage"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 186px; height: 257px;" src="' + this.ShowImageUrl + '"></a></div><div class="blurbWatchNow"><h4 style="width:200px;">' + showname + '</h4><p>';
                //                            shtml += this.Blurb;
                //                            shtml += '</p><p><a class="watchnow watchnowmovie" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div></div>';
                //                        }
                //                    }
                //                });
                //                if (featureLength>1)
                //                    shtml += '<div class="moreLink"><a href="/Show/Details/' + this.id + '" class="more">All Episodes&raquo;</a></div>';
                shtml += '</li>';
            });
            if (id == "menuMovies")
                shtml += '<div class="moreLink"><a href="/Category/List/' + this.id + '" class="more">All ' + this.name + ' Movies&raquo;</a></div>';
            else if (id == "menuLive")
                shtml += '<div class="moreLink"><a href="/Category/List/' + this.id + '" class="more">All ' + this.name + ' &raquo;</a></div>';
            else
                shtml += '<div class="moreLink"><a href="/Category/List/' + this.id + '" class="more">All ' + this.name + ' Shows&raquo;</a></div>';
            shtml += '</ul></div></li>';
        });
        shtml += '</ul></div>';
        var id2 = "#source" + id;
        $(id2).html(shtml);

    });
}

function getEpisodeData(episodeData, menuType, showName, categoryId) {
    var shtml = '<div class="menuListHeader episodeHeader menuListheaderPixelbar">';
    if (menuType == "menuEntertainment" || menuType == "menuNews")
        shtml += '<h3>LATEST EPISODES</h3>';
    else if (menuType == "menuMovies")
        shtml += '<h3>FEATURED MOVIE</h3>';
    else shtml += '<h3>LIVE STREAMING</h3>';
    shtml += '</div>';
    var featureLength = episodeData.length;
    $.each(episodeData, function (x, y) {
        if (x == 0) {
            if (featureLength > 1) {
                shtml += '<div class="dsourceEpisodes sourceEpLeft"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 151px; height: 98px;" src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>';
                shtml += this.Blurb;
                shtml += '</p><p><a class="watchnow" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div>';
            }
            else {
                shtml += '<div class="dsourceEpisodes dsourceMovieEpisode sourceEpLeft"><div class="posterImage"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 186px; height: 257px;" src="' + this.ShowImageUrl + '"></a></div><div class="blurbWatchNow"><h4 style="width:200px;">' + showName + '</h4><p>';
                shtml += this.Blurb;
                shtml += '</p><p><a class="watchnow watchnowmovie" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div></div>';
            }
        } else {
            if (featureLength > 1) {
                shtml += '<div class="dsourceEpisodes"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 151px; height: 98px;" src="' + this.EpisodeImageUrl + '"></a><p>Date Aired:' + this.EpisodeAirDate + '</p><p>';
                shtml += this.Blurb;
                shtml += '</p><p><a class="watchnow" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div>';
            }
            else {
                shtml += '<div class="dsourceEpisodes dsourceMovieEpisode"><div class="posterImage"><a href="/Episode/Details/' + this.EpisodeId + '"><img style="width: 186px; height: 257px;" src="' + this.ShowImageUrl + '"></a></div><div class="blurbWatchNow"><h4 style="width:200px;">' + showName + '</h4><p>';
                shtml += this.Blurb;
                shtml += '</p><p><a class="watchnow watchnowmovie" href="/Episode/Details/' + this.EpisodeId + '"></a></p></div></div>';
            }
        }
    });
    if (featureLength > 1)
        shtml += '<div class="moreLink"><a href="/Show/Details/' + this.id + '" class="more">All Episodes&raquo;</a></div></div>';
    $('#' + categoryId).html(shtml);
    $('.dEpisodes').html($('#' + categoryId).html());
}