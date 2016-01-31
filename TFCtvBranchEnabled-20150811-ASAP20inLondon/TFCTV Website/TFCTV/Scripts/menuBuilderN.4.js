
function getEpisodeData(episodeData, menuType, showName, categoryId) {
    var shtml = '<div class="menuListHeader episodeHeader menuListheaderPixelbar">';
    if (menuType == "limenuEntertainment" || menuType == "limenuNews" || menuType == "menuEntertainment" || menuType == "menuNews")
        shtml += '<h3>LATEST EPISODES</h3>';
    else if (menuType == "limenuMovies" || menuType == "menuMovies")
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
        shtml += '<div class="moreLink"><a href="/Show/Details/' + categoryId + '" class="more">All ' + ellipsis(showName) + ' Episodes &raquo;</a></div></div>';
    $('#' + categoryId).html(shtml);
}


function attachAIM(parent) {

    $('#ulGenre' + parent).menuAim({
        activate: activateMenu,
        deactivate: deactivateMenu
    });

    function activateMenu(row) {
        $('.liGenre').removeClass('selectedSubMenu');
        $('.dShows').addClass('hideElement');
        $('.liShows').removeClass('selectedShowMenu');
        $('.dEpisodes').addClass('hideElement');

        var label = $(row).attr('id');
        $(row).addClass('selectedSubMenu');
        $(row).find('.liShows:first').addClass('selectedShowMenu');
        $(row).find('.dShows:first').removeClass('hideElement')
        var firstepisodes = $(row).find('.dEpisodes:first');
        if (firstepisodes.html().trim().length == 0) {
            var menutype = $('.selectedMenu').attr('id');
            var showname = $('.selectedShowMenu>a>span').text();
            var categoryId = firstepisodes.attr('id');
            jQuery.get('/Menu/GetPreviewEpisodes/' + categoryId, function (episodeData) { getEpisodeData(episodeData, menutype, showname, categoryId) });
        }
        $(firstepisodes).removeClass('hideElement');
        attachAIMShows(label);
    }

    function deactivateMenu(row) {
        $(row).find('.liShows:first').removeClass('selectedShowMenu');
        $(row).find('.dShows:first').addClass('hideElement')
        $(row).find('.dEpisodes').addClass('hideElement')
    }
}
function attachAIMShows(id) {

    $('#ulShows' + id).menuAim({
        activate: activateMenu,
        deactivate: deactivateMenu
    });

    function activateMenu(row) {
        $('.dEpisodes').addClass('hideElement');
        $('.liShows').removeClass('selectedShowMenu');

        $(row).addClass('selectedShowMenu');
        var episodesPrev = $(row).find('.dEpisodes');
        if (episodesPrev.html().trim().length == 0) {
            var menutype = $('.selectedMenu').attr('id');
            var showname = $('.selectedShowMenu>a>span').text();
            var categoryId = episodesPrev.attr('id');
            jQuery.get('/Menu/GetPreviewEpisodes/' + categoryId, function (episodeData) { getEpisodeData(episodeData, menutype, showname, categoryId) });
        }
        $(episodesPrev).removeClass('hideElement');
    }

    function deactivateMenu(row) {
        $(row).removeClass('selectedShowMenu');
        $(row).find('.dShows:first').addClass('hideElement')
        $(row).find('.dEpisodes').addClass('hideElement')
    }
}

jQuery.fn.hoverWithDelay = function (inCallback, outCallback, delay) {
    this.each(function () {
        var timer, $this = this;
        $(this).hover(function () {
            timer = setTimeout(function () {
                timer = null;
                inCallback.call($this);
            }, delay);
        }, function () {
            if (timer) {
                clearTimeout(timer);
                timer = null;
            } else
                outCallback.call($this);
        });
    });
};

function ellipsis(text) {
    text = text.trim();
    if (text.length > 45) {
        text = text.substring(0, 42);
        text = text.substring(0, text.lastIndexOf(" ")) + "...";
    }
    return text;
}