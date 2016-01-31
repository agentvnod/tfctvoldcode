function shows(param) {
    var html = "";
    var temp = "";
    var i = 0;
    $.each(param, function (x, y) {
        if (y.MainCategoryId != temp) {
            if (i == 1) {
                html += '</div>';
                html += '</div>';
                i = 0;
            }
            html += '<div class="showcategory_container" catid="' + y.MainCategoryId + '" >'
            html += '<div class="cat_title"><a href="/Category/List/' + y.MainCategoryId + ' ">' + y.MainCategory + '</a></div>'
            html += '<div class="showlist">';
            temp = y.MainCategoryId;
        }
        if (y.MainCategoryId == temp) {
            html += '<a href="/show/details/' + y.ShowId + '">' + y.Show + '</a><br/>';
            i = 1;
        }
    });
    $('#ShowContainer').html(html);
}

function GetProductsOnClick(p) {
    var u = '/Packages/GetProducts?packageid=' + p;
    jQuery.get(u, function (data) {
        products(data, p);
    }, 'json');

    url = '/Packages/GetShows?packageid=' + p;
    $.ajax({
        url: url
		        , dataType: 'json'
		        , beforeSend: function () {
		            $('#spanner').addClass('feature_spanner');
		            $('#ShowContainer').html($('#ajax-loading').html());
		        }
		        , success: function (data) {
		            shows(data);
		        }
    });
}