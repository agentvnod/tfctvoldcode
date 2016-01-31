var comments = function (params) {
    this.listing = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize        
        $.each(params.json_str, function (x, y) {
            var c = y.commentText;
            if (ctr == 1)
                html += '<ul class="commentListing">';
            html += '<li>';
            html += '<div class="commentContainer"><div class="floatLeft imgBox">';
            if (y.sender.photoURL != undefined)
                html += '<img src="' + y.sender.photoURL + '" />';
            else
                html += '<img src="http://cdnassets.tfc.tv/content/images/social/profilephoto.png" width="50" height="50" />';
            html += '</div><div class="floatLeft commentBox"><div>';
            html += '<span class="commentName">' + y.sender.name + '</span>';
            html += '&nbsp;&nbsp;<span class="commentDate">posted ' + jQuery.timeago(y.timestamp) + '</span></div>';
            html += '<div class="breakSmall"></div>';
            html += '<div class="commentBody">' + c.split(/\s+/).slice(0, 32).join(" ") + '...</div>';
            html += '</div><div class="clear"></div></div>';
            html += '</li>';
            if (ctr == params.itemperslide) {
                html += '</ul>'; ctr = 0; //reset counter
            }
            ctr++;
        });
        var json_length = params.json_str.length;
        $('#' + params.container + ' .itemListBody .wrapper').html(html);
        if (json_length > params.itemperslide) {
            $('#' + params.container + ' .itemListBody .wrapper').cycle({ fx: 'scrollDown' });
        }
    }

    this.paginated = function () {
        var ctr = 1; //item counter
        var html = ''; //initialize
        var nav_html = ''; // initialize
        if (params.append)
            html += '<ul class="commentListing hideElement">';
        else
            html += '<ul class="commentListing">';
        $.each(params.json_str, function (x, y) {
            var c = y.commentText;
            html += '<li>';
            html += '<div class="comment-container-full"><div class="floatLeft imgBox">';
            if (y.sender.photoURL != undefined)
                html += '<img src="' + y.sender.photoURL + '" />';
            else
                html += '<img src="http://cdnassets.tfc.tv/content/images/social/profilephoto.png" width="50" height="50" />';
            html += '</div><div class="floatLeft commentBox"><div>';
            html += '<span class="commentName">' + y.sender.name + '</span>';
            html += '&nbsp;&nbsp;<span class="commentDate">posted ' + jQuery.timeago(y.timestamp) + '</span></div>';
            html += '<div class="breakSmall"></div>';
            html += '<div class="commentBody">' + c.split(/\s+/).slice(0, 32).join(" ") + '...</div>';
            html += '</div><div class="clear"></div></div>';
            html += '</li>';
        });
        html += '</ul>';
        if (params.append) {
            $(html).appendTo('#' + params.container + ' .itemListBody .wrapper').fadeIn('slow');
            //$('#' + params.container + ' .itemListBody .wrapper').show('slow').append(html).fadeIn('slow');
        }
        else
            $('#' + params.container + ' .itemListBody .wrapper').html(html);
    }
}