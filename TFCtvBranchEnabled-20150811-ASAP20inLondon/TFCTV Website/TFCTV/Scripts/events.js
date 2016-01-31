var $ = jQuery; // no config mode

$(document).ready(function () {
    // === auto rotating images in Independence Landing Page
    $(window).load(function () {
        var InfiniteRotator =
        {
            init: function () {
                //initial fade-in time (in milliseconds)
                var initialFadeIn = 1000;

                //interval between items (in milliseconds)
                var itemInterval = 8000;

                //cross-fade time (in milliseconds)
                var fadeTime = 2500;

                //count number of items
                var numberOfItems = $('.rotating-item').length;

                //set current item
                var currentItem = 0;

                //show first item
                $('.rotating-item').eq(currentItem).fadeIn(initialFadeIn);

                //loop through the items
                var infiniteLoop = setInterval(function () {
                    $('.rotating-item').eq(currentItem).fadeOut(fadeTime);

                    if (currentItem == numberOfItems - 1) {
                        currentItem = 0;
                    } else {
                        currentItem++;
                    }
                    $('.rotating-item').eq(currentItem).fadeIn(fadeTime);

                }, itemInterval);
            }
        };

        InfiniteRotator.init();
    });
    // END auto rotating images ...    
    $("html").niceScroll({ autohidemode: false, cursorwidth: '10px', cursorborderradius: 0, cursorborder: 0, cursorcolor: '#AAAAAB', background: '#F1F1F1', spacebarenabled: false });
});  // end .ready() function

function createCookie(name, value, days) {
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + "; path=/";
}