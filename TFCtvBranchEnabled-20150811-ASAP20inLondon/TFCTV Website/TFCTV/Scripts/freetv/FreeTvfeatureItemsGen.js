var FreeTvfeatureItem = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize        

        $.each(params.json_str, function (x, y) {
            if (params.type == 'show') {
                html += '<div class="clip-element item">';
                html += '<div class="clip-thumbnail">';
                html += '<div class="clip-photo">';
                html += '<a epId="' + y.EpisodeId + '" href="/freetv/' + params.playlistId + '/' + y.EpisodeId + '">';
                html += '<img src="' + y.EpisodeImageUrl + '" title="' + y.Blurb + '" border="0" width="130" />';
                html += '</a>';
                html += '</div>';
                html += '</div>';
                html += '<div class="clip-info">';
                html += '<span class="text-show-info-episode-title"><a epId="' + y.EpisodeId + '"  href="/freetv/' + params.playlistId + '/' + y.EpisodeId + '">' + y.ShowName + '</a> </span><br/>';
                html += '<span class="white">Date Aired: ' + y.EpisodeAirDate + '</span>';
                html += '<br />';
                html += '</div>';
                html += '<div class="clear">';
                html += '</div>';
                html += '</div>';
            }
            if (params.type == "category") {
                html += '<li class="show-preview has-tool-tip">';
                html += '<div class="show-thumb">';
                html += '<img alt="" class="image-show-info-thumb person-photo" src="' + y.EpisodeImageUrl + '"  title="' + y.Blurb + '"  height="98" width="151" />';
                html += '</div>';
                html += '<div class="show-info">';
                html += '<span class="text-show-info-episode-title"><a epId="' + y.EpisodeId + '" href="/freetv/' + params.playlistId + '/' + y.EpisodeId + '">' + y.ShowName + '</a> </span>';
                html += '<br />';
                html += '<span class="text-show-info-episode-airdate">Date Aired:' + y.EpisodeAirDate + '</span><br />';
                html += '</div>';
                html += '</li>';
            }
        });
        if (params.type == "category") {
            $('#' + params.container + ' .featuredItemsTab').html(html);            
        } else {
            $('#' + params.container + ' .featuredItems').html(html);
        }
    }
}