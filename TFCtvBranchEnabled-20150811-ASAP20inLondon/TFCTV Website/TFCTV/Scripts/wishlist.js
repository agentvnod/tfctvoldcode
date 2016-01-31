/* File Created: February 10, 2012 */
var wishlist = function (params) {
    this.create = function () {
        var html = '';
        html += '<ul>';

        if (params.data.length > 0) {
            $.each(params.data, function (x, y) {
                html += '<li id="li-' + y._id + '"><span class="lightBlue boldText"><a rel="#overlay" class="wlink" id="' + y._id + '" href="/Buy/Process/' + y.ProductId_i + '?wid=' + y._id + '">' + y.ProductName_s + '</a></span>';
                html += ' added on <span class="darkBlue">' + Date.parse(y.registDt_d).toString('dddd, MMMM d, yyyy hh:mm:ss tt') + '</span>';
                if (params.options)
                    html += '<span class="wl-option floatRight"><a href="#" rel="' + y._id + '" class="wl-delete">x</a></span>';
                html += '</li>';
            });
        }
        else
            html += '<li>No wishlist found.</li>';

        html += '</ul>';
        $('#' + params.container).html(html);
    }
}