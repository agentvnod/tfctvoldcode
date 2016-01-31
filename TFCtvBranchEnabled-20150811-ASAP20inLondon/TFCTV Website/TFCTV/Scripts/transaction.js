/* File Created: August 5, 2013 */
$(document).ajaxError(function (event, jqxhr, settings, exception) {
    setTimeout(function () {
        $('#msgbox-content').html("Error requesting page: " + settings.url);
        $('#wait-time').fadeOut(500, function () {
            $('#errorContainer').fadeIn();
            $('html, body').animate({
                scrollTop: $('#entertainmentBtn').offset().top
            }, 800);
        });
    }, 800);
});