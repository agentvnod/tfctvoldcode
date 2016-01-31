/* File Created: February 2, 2012 */
var menu = function (params) {
    this.create = function () {
        var html = '';
        $.each(params.json_str, function () {
            html += '<div class="showList">';
            html += '<div class="categoryTitle floatLeft">';
            if (this.type == 1) {
                //html += '<a href="/Channel/List/' + this.id + '">' + this.name + '</a></div>';
                html += '<a href="/Live/Index/'+ this.name.replace(' ','') + '">' + this.name + '</a></div>';
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
