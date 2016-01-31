var pagination = function (params) {
    this.listing = function () {
        var html = '';
        var nav_html = "";
        var nav_max = 10;
        var nav_min = 1;

        if (params.json_str.CurrentPage / 2 > 3) {
            nav_min = params.json_str.CurrentPage - 3;
            nav_max = params.json_str.CurrentPage + 6;
        }

        $.each(params.json_str.Data, function (x, y) {
            html += '<div class="showItem_preview"><div class="imgShowThumbBg">';
            html += '<img src="http://asset1.tfctvapp.com/images/angelito/20120113-angelito-151x98.jpg" alt=""/>';
            html += '</div><div class="itemInfo">';
            html += '<span class="showTitle"><a href="/Episode/Details/' + y.EpisodeId + '">' + y.ShowName + '</a></span><br />';
            html += '<span class="white">' + y.EpisodeDescription + '</span><br />';
            html += '<span class="white">Airdate: ' + y.EpisodeAirDate + '</span></div><div class="clear"></div></div>';
        });
        nav_html += '<div id="pg">';
        if (params.json_str.CurrentPage == 1) {
            nav_html += "<span class='disabled'>&laquo; Previous</span>";
        } else {
            nav_html += "<a href='#' onclick='paginate(" + params.json_str.CurrentPage + ",1,0)'>&laquo; Previous</a>";
        }

        for (i = nav_min; i <= (nav_max); i++) {
            if (i == params.json_str.CurrentPage) {
                nav_html += '<a class="current" page="' + i + '" href="#"  onclick="paginate(' + i + ',0,0)" >' + i + '</a>';
            } else {
                nav_html += '<a page="' + i + '" href="#"  onclick="paginate(' + i + ',0,0)" >' + i + '</a>';
            }
        }


        if (params.json_str.CurrentPage == params.json_str.NumberOfPages) {
            nav_html += "<span class='disabled'>Next &raquo;</span>";
        }
        else {
            nav_html += "<a href='#'  onclick='paginate(" + params.json_str.CurrentPage + ",0,1)'>Next &raquo;</a>";
        }

        nav_html += '</div>';

        $('#' + params.container).html(html + "<div class='clear'></div><br/><br/>");
        $('#' + 'navigation').html(nav_html);


    }
}