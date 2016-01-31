/* File Created: February 9, 2012 */
function lightsOut() { $('#lighstout').fadeIn(1000); }
function lightsOn() { $('#lighstout').fadeOut(); }
function scrollTo(id) {
    $('html,body').animate({ scrollTop: $(id).offset().top }, 'slow');
}
function reloadPage() { window.location.reload(); }
function redirectToPage(url) { window.location.href = url; }
function generateCarousels(item, withToolTip) {
    $.ajax({
        url: item.url
                    , dataType: 'json'
                    , beforeSend: function () {
                        $('#spanner').addClass('feature_spanner');
                        $('#spanner').css('height', '130px');
                        $('#' + item.container + ' .itemListBody .wrapper').empty();
                        $('#' + item.container + ' .itemListBody .wrapper').prepend($('#ajax-loading').html());
                    }
                    , success: function (data) {
                        var list = new featureItem({
                            json_str: data, type: item.type, itemperslide: item.itemperslide, container: item.container, nid: item.nid
                        }).listing();
                        if (withToolTip)
                            $('.itemListing li img[title]').tooltip({ effect: 'fade',
                                onShow: function () {
                                    var tip = this.getTip();
                                    setTimeout(function () {
                                        tip.hide();
                                    }, 10000);
                                }
                            });
                    }
    });
}
$.fn.textWidth = function () {
    var html_org = $(this).html();
    var html_calc = '<span>' + html_org + '</span>';
    $(this).html(html_calc);
    var width = $(this).find('span:first').width();
    $(this).html(html_org);
    return width;
};

function createError(value) {
    var e = '';
    //    e += '<div class="ui-widget" style="width: 250px; font-size: 11px;">';
    //    e += '<div class="ui-state-error ui-corner-all" style="padding: 0 .7em;">';
    //    e += '<p><span class="ui-icon ui-icon-alert" style="float: left; margin-right: .3em;"></span>';
    //    e += value;
    //    e += '</p></div></div>';
    e += '<span class="errorText smallRoundCorners">';
    e += value;
    e += '</span>';
    return e;
}

function createHighlight(value) {
    var e = '';
    //    e += '<div class="ui-widget" style="width: 250px; font-size: 11px;">';
    //    e += '<div class="ui-state-highlight ui-corner-all" style="margin-top: 20px; padding: 0 .7em;">';
    //    e += '<p><span class="ui-icon ui-icon-info" style="float: left; margin-right: .3em;"></span>';
    //    e += value;
    //    e += '</p></div></div>';
    e += '<span class="highlightText smallRoundCorners">';
    e += value;
    e += '</span>';
    return e;
}

function getUserBalance() {
    $.get('/User/GetBalance', function (data) {
        $('.user-credits').html(data.balance);
    });
}

function changeRatingsAttribute() {
    var ratings = ['It could be better', 'It\'s okay','I like it','I\'m hooked','I\'m in love'];
    $('.gig-comments-star-editable').each(function (x) {
        $(this).attr('gig-rating-details', ratings[x]);        
    });
}

 
// ----------------------------------------------------------
// If you're not in IE (or IE version is less than 5) then:
//     ie === undefined
// If you're in IE (>5) then you can determine which version:
//     ie === 7; // IE7
// Thus, to detect IE:
//     if (ie) {}
// And to detect the version:
//     ie === 6 // IE6
//     ie> 7 // IE8, IE9 ...
//     ie <9 // Anything less than IE9
// ----------------------------------------------------------
var ie = (function(){
    var undef, v = 3, div = document.createElement('div');
   
    while (
        div.innerHTML = '<!--[if gt IE '+(++v)+']><i></i><![endif]-->',
        div.getElementsByTagName('i')[0]
    );
   
    return v> 4 ? v : undef;
}());
 

