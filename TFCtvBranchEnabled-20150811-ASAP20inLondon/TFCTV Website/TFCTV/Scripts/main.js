var $ = jQuery; // no config mode

$(document).ready(function () {

    $(document).click(function (e) {
        var search_button = $('#search_buttoni');  // hide search box on outside click
        var search_box = $('.search_box');
        if (!search_button.is(e.target) && search_button.has(e.target).length === 0
        && !search_box.is(e.target) && search_box.has(e.target).length === 0) {
            $('.search_box').hide(300);
        }

        var login_button = $('#login_buttoni');   // hide Login box on outside click
        var login_box = $('.login_box');
        var composebox_site = $('.gig-composebox-site-login');  // comment login button
        var composebox = $('.gig-composebox-post');     // comment post button
        var login_boxa = $('.login_boxa'); // general element to avoid hide login box

        if (!login_button.is(e.target) && login_button.has(e.target).length === 0
        && !login_box.is(e.target) && login_box.has(e.target).length === 0
        && !composebox_site.is(e.target) && composebox_site.has(e.target).length === 0
        && !composebox.is(e.target) && composebox.has(e.target).length === 0
        && !login_boxa.is(e.target) && login_boxa.has(e.target).length === 0) {
            $('.login_box').hide(300);
        }

        var setting_button = $('#setting_buttoni');  // hide setting box on outside click
        var setting_box = $('.setting_box');
        if (!setting_button.is(e.target) && setting_button.has(e.target).length === 0
        && !setting_box.is(e.target) && setting_box.has(e.target).length === 0) {
            $('.setting_box').hide(300);
        }

    });

    // show search box
    $('#search_buttoni').click(function () {
        $('.search_box').toggle(300);
    });

    // show login box
    $('#login_buttoni').click(function () {
        $('.login_box').toggle(300);
    });

    // show setting box
    $('#setting_buttoni').click(function () {
        $('.setting_box').toggle(300);
    });

    // hide any popupbox when menu or mobile menu is overd or clicked
    $('#main_menu a, .navbar a, .navbar-toggle').on('click mouseover', function () {
        $('.setting_box').hide(300);
        $('.search_box').hide(300);
        $('.login_box').hide(300);
    });

    // social shareing button [make tabel responsive]
    $('.gig-bar-container').addClass('table-responsive');
    $('.gig-bar-container table').addClass('table');


    // back to top
    $('#top-link-block a').click(function (e) {
        e.preventDefault();
        $('html,body').animate({ scrollTop: 0 }, 'slow'); return false;
    });

    // close one level bar
    $('.ib_close').click(function (e) {
        e.preventDefault();
        $(this).parent().hide(400);
    });

    // apper home page seccessful message 
    //$('#regSceMsg').modal('show');

    // 3 option box in [subscription.html]
    $('.offerboxs .offerbox').click(function () {
        $('.offerboxs .offerbox').removeClass('active');
        $(this).addClass('active');
    });

    // close processing msg in 2 second [subscription.html]
    /*$('.sub_pro_msgb').click(function(){
        setTimeout(function() {$('#subProMsg').modal('hide');}, 2000);
    });*/

    // change state on Country change [register page]	
    changestate($('#country').val()); // call on page load
    $('#country').change(function () {   // call on change
        changestate(this.value); // call function		
    });


    // for touch screen     
    //$( '#main_menu li:has(ul)' ).doubleTapToGo();
    $('#main_menu li').doubleTapToGo();

    // change background images [F:17]
    $('#background_cycler').hide();//hide the background while the images load, ready to fade in later
    $('#background_cycler').fadeIn(1500);//fade the background back in once all the images are loaded	
    if ($("#background_cycler .dott").length > 0) {
        $('#background_cycler .dott img:first').attr('src', $('#background_cycler .dott img:first').attr('src').replace('gray', 'white')).parent().addClass("active");
    }

    var ctimer;
    function startCycle() {
        ctimer = setInterval('cycleImages()', 7000);  // run every 7s
    }
    $("#background_cycler .arrows img").hover(function (e) {
        clearInterval(ctimer);
    }, function (e) {
        startCycle();
    });

    $("#background_cycler .dots .dott img").hover(function (e) {
        clearInterval(ctimer);
    }, function (e) {
        startCycle();
    });

    $("#background_cycler .bimg").hover(function (e) {
        clearInterval(ctimer);
    }, function (e) {
        startCycle();
    });

    startCycle();
    //setInterval('cycleImages()', 7000);  // run every 7s

    // one of two is required [tve-activation.html]
    $('.pay_optg').change(function () {
        var name = $(this).attr('name');
        var value = $(this).val();
        if (name == "smartCardNum" && value != "") {
            $("#cusAccount").removeAttr("required");
            $("#cusAccount").val("");
        } else if (name == "cusAccount" && value != "") {
            $("#smartCardNum").removeAttr("required");
            $("#smartCardNum").val("");
        }

        if (name == "smartCardNum" && value == "") {
            $("#cusAccount").attr("required", "required");
        } else if (name == "cusAccount" && value == "") {
            $("#smartCardNum").attr("required", "required");
        }
    });
    // END required ...

    // change video on select item change [halo-halo-clicks-videos.html] [F:16]
    haloHaloClickChangeVideo();
    $('#hhcv_sortby').change(function () {
        haloHaloClickChangeVideo();
    });
    // END halo-halo on change....

    // show all for desktop menu 
    /*$(".ib_marea .showall").each(function() {
   
       var $link = $(this);
       var $content = $link.prev(".collapse_menu");
       
       var $num_items = $content.children().size();
       if($num_items > 6){
           $content.children().hide();
           $content.children('p:lt(6)').show();
       }else{
           $link.hide();
       }
   });
   
   $('.ib_marea .showall').click(function (e) {
       e.preventDefault();
       var $link = $(this);
       var $content = $link.prev(".collapse_menu");
       var linkText = $link.html();
       
       if($content.hasClass('short-menu')){
           $content.children().show();
       }else{
           $content.children('p:gt(5)').hide();
       }
       $content.toggleClass("short-menu full-menu");
       $link.html(getShowLinkText_menu(linkText));
   });*/

    function getShowLinkText_menu(currentText) {
        var newText = '';

        if (currentText === '<a href="#">Show few &lt;&lt;</a>') {
            newText = '<a href="#">Show ALL &gt;&gt;</a>';
        } else {
            newText = '<a href="#">Show few &lt;&lt;</a>';
        }

        return newText;
    }
    // END show all for desktop menu

    // Show/hide more/less content in show page
    $(".ib-show-more").each(function () {
        var $link = $(this);
        var $content = $link.prev(".text-content");

        //console.log($link);

        var visibleHeight = $content[0].clientHeight;
        var actualHide = $content[0].scrollHeight - 1;

        //console.log(actualHide);
        //console.log(visibleHeight);

        if (actualHide > (visibleHeight + 10)) {
            $link.show();
        } else {
            $link.hide();
        }
    });

    $(".ib-show-more").on("click", function (e) {
        e.preventDefault();
        var $link = $(this);
        var $content = $link.prev(".text-content");
        var linkText = $link.html();

        $content.toggleClass("short-text, full-text");

        $link.html(getShowLinkText(linkText));

        return false;
    });

    function getShowLinkText(currentText) {
        var newText = '';

        if (currentText === "less" + ' <span class="glyphicon glyphicon-chevron-up"></span>') {
            newText = "more" + ' <span class="glyphicon glyphicon-chevron-down"></span>';
        } else {
            newText = "less" + ' <span class="glyphicon glyphicon-chevron-up"></span>';
        }

        return newText;
    }
    // END Show/hide more/less content in show page

    var size_li = $(".everyones_activity .feed_item").size();
    x = 5;
    $(".everyones_activity .feed_item").hide();
    $(".everyones_activity .feed_item:lt(" + x + ")").fadeIn('slow');
    $('.everyones_activity .loadMore').click(function (e) {
        e.preventDefault();
        x = (x + 5 <= size_li) ? x + 5 : size_li;
        $('.everyones_activity .feed_item:lt(' + x + ')').fadeIn();
        if (x >= size_li)
            $(this).hide();
    });

    var size_li2 = $(".sec_friends .feed_item").size();
    x2 = 8;
    $(".sec_friends .feed_item").hide();
    $(".sec_friends .feed_item:lt(" + x2 + ")").fadeIn('slow');
    $('.sec_friends .loadMore').click(function (e) {
        e.preventDefault();
        x2 = (x2 + 8 <= size_li2) ? x2 + 8 : size_li2;
        $('.sec_friends .feed_item:lt(' + x2 + ')').fadeIn();
        if (x2 >= size_li2)
            $(this).hide();
    });

    var size_li3 = $(".sec_loved_shows .feed_item").size();
    x3 = 6;
    $(".sec_loved_shows .feed_item").hide();
    $(".sec_loved_shows .feed_item:lt(" + x3 + ")").fadeIn('slow');
    $('.sec_loved_shows .loadMore').click(function (e) {
        e.preventDefault();
        x3 = (x3 + 6 <= size_li3) ? x3 + 6 : size_li3;
        $('.sec_loved_shows .feed_item:lt(' + x3 + ')').fadeIn();
        if (x3 >= size_li3)
            $(this).hide();
    });

    var size_li4 = $(".sec_loved_celebrities .feed_item").size();
    x4 = 6;
    $(".sec_loved_celebrities .feed_item").hide();
    $(".sec_loved_celebrities .feed_item:lt(" + x4 + ")").fadeIn('slow');
    $('.sec_loved_celebrities .loadMore').click(function (e) {
        e.preventDefault();
        x4 = (x4 + 6 <= size_li4) ? x4 + 6 : size_li4;
        $('.sec_loved_celebrities .feed_item:lt(' + x4 + ')').fadeIn();
        if (x4 >= size_li4)
            $(this).hide();
    });

    var size_li5 = $(".sec_tab_friends .feed_item").size();
    x5 = 5;
    $(".sec_tab_friends .feed_item").hide();
    $(".sec_tab_friends .feed_item:lt(" + x5 + ")").fadeIn('slow');
    $('.sec_tab_friends .loadMore').click(function (e) {
        e.preventDefault();
        x5 = (x5 + 5 <= size_li5) ? x5 + 5 : size_li5;
        $('.sec_tab_friends .feed_item:lt(' + x5 + ')').fadeIn();
        if (x5 >= size_li5)
            $(this).hide();
    });

    var size_li6 = $(".sec_tab_me .feed_item").size();
    x6 = 5;
    $(".sec_tab_me .feed_item").hide();
    $(".sec_tab_me .feed_item:lt(" + x6 + ")").fadeIn('slow');
    $('.sec_tab_me .loadMore').click(function (e) {
        e.preventDefault();
        x6 = (x6 + 5 <= size_li6) ? x6 + 5 : size_li6;
        $('.sec_tab_me .feed_item:lt(' + x6 + ')').fadeIn();
        if (x6 >= size_li6)
            $(this).hide();
    });

    var size_li7 = $(".table_subscriptions tbody tr").size();
    x7 = 5;
    $(".table_subscriptions tbody tr").hide();
    $(".table_subscriptions tbody tr:lt(" + x7 + ")").fadeIn('slow');
    $('.table_subscriptions .loadMore').click(function (e) {
        e.preventDefault();
        x7 = (x7 + 5 <= size_li7) ? x7 + 5 : size_li7;
        $('.table_subscriptions tbody tr:lt(' + x7 + ')').fadeIn();
        if (x7 >= size_li7)
            $(this).hide();
    });

    var size_li8 = $(".table_transactions tbody tr").size();
    x8 = 10;
    $(".table_transactions tbody tr").hide();
    $(".table_transactions tbody tr:lt(" + x8 + ")").fadeIn('slow');
    $('.table_transactions .loadMore').click(function (e) {
        e.preventDefault();
        x8 = (x8 + 10 <= size_li8) ? x8 + 10 : size_li8;
        $('.table_transactions tbody tr:lt(' + x8 + ')').fadeIn();
        if (x8 >= size_li8)
            $(this).hide();
    });

    var size_li9 = $(".renewal_subscription_table tbody tr").size();
    x9 = 10;
    $(".renewal_subscription_table tbody tr").hide();
    $(".renewal_subscription_table tbody tr:lt(" + x9 + ")").fadeIn('slow');
    $('.renewal_subscription_table .loadMore').click(function (e) {
        e.preventDefault();
        x9 = (x9 + 10 <= size_li9) ? x9 + 10 : size_li9;
        $('.renewal_subscription_table tbody tr:lt(' + x9 + ')').fadeIn();
        if (x9 >= size_li9)
            $(this).hide();
    });


    // LATEST EPISODE SHOW MORE
    var size_li_le = $(".latest_episodes .movie").size();
    x10 = 12;
    if (x10 >= size_li_le)
        $('.latest_episodes .loadMore').hide();
    $(".latest_episodes .movie").hide();
    $(".latest_episodes .movie:lt(" + x10 + ")").fadeIn('slow');
    $('.latest_episodes .loadMore').click(function (e) {
        e.preventDefault();
        //console.log('.latest_episodes .movie:lt('+x10+')');
        x10 = (x10 + 12 <= size_li_le) ? x10 + 12 : size_li_le;
        $('.latest_episodes .movie:lt(' + x10 + ')').fadeIn();
        if (x10 >= size_li_le)
            $(this).hide();
    });

    // POPULAR EPISODE SHOW MORE
    var size_li_pot = $(".popular_episodes .movie").size();
    x11 = 8;
    if (x11 >= size_li_pot)
        $('.popular_episodes .loadMore').hide();
    $(".popular_episodes .movie").hide();
    $(".popular_episodes .movie:lt(" + x11 + ")").fadeIn('slow');
    $('.popular_episodes .loadMore').click(function (e) {
        e.preventDefault();
        x11 = (x11 + 8 <= size_li_pot) ? x11 + 8 : size_li_pot;
        $('.popular_episodes .movie:lt(' + x11 + ')').fadeIn();
        if (x11 >= size_li_pot)
            $(this).hide();
    });

    // LATEST SHOWS SHOW MORE
    var size_li_ls = $(".latest_shows .movie").size();
    x12 = 6;
    if (x12 >= size_li_ls)
        $('.latest_shows .loadMore').hide();
    $(".latest_shows .movie").hide();
    $(".latest_shows .movie:lt(" + x12 + ")").fadeIn('slow');
    $('.latest_shows .loadMore').click(function (e) {
        e.preventDefault();
        x12 = (x12 + 6 <= size_li_ls) ? x12 + 6 : size_li_ls;
        $('.latest_shows .movie:lt(' + x12 + ')').fadeIn();
        if (x12 >= size_li_ls)
            $(this).hide();
    });

    // FEATURED CELEBRITY SHOW MORE
    var size_li_fs = $(".featured_celebrity .movie").size();
    x13 = 6;
    if (x13 >= size_li_fs)
        $('.featured_celebrity .loadMore').hide();
    $(".featured_celebrity .movie").hide();
    $(".featured_celebrity .movie:lt(" + x13 + ")").fadeIn('slow');
    $('.featured_celebrity .loadMore').click(function (e) {
        e.preventDefault();
        x13 = (x13 + 6 <= size_li_fs) ? x13 + 6 : size_li_fs;
        $('.featured_celebrity .movie:lt(' + x13 + ')').fadeIn();
        if (x13 >= size_li_fs)
            $(this).hide();
    });

    // CATEGORY PAGE
    $('.category_section .movie_sec').each(function (x, y) {
        var d_size = $(this).find('.movie').size();
        x14 = 12;
        if (x14 >= d_size)
            $(this).find('.loadMore').hide();
        $(this).find(".movie").hide();
        $(this).find(".movie:lt(" + x14 + ")").fadeIn('slow');
        sec = $(this);
        $(this).find('.loadMore').click(function (e) {
            e.preventDefault();
            x14 = (x14 + 12 <= d_size) ? x14 + 12 : d_size;
            $($(this).parent().parent()).find('.movie:lt(' + x14 + ')').fadeIn();
            if (x14 >= d_size)
                $(this).hide();
        });
    });

    // CATEGORY SECTION LIST SHOW MORE
    var size_li_csl = $(".category_section_list .movie").size();
    x15 = 42;
    if (x15 >= size_li_csl)
        $('.category_section_list .loadMore').hide();
    $(".category_section_list .movie").hide();
    $(".category_section_list .movie:lt(" + x15 + ")").fadeIn('slow');
    $('.category_section_list .loadMore').click(function (e) {
        e.preventDefault();
        x15 = (x15 + 42 <= size_li_csl) ? x15 + 42 : size_li_csl;
        $('.category_section_list .movie:lt(' + x15 + ')').fadeIn();
        if (x15 >= size_li_csl)
            $(this).hide();
    });

    // CELEBRITY PROFILE
    $('.celebrity_section .movie_sec').each(function (x, y) {
        var d_size = $(this).find('.movie').size();
        x16 = 6;
        if (x16 >= d_size)
            $(this).find('.loadMore').hide();
        $(this).find(".movie").hide();
        $(this).find(".movie:lt(" + x16 + ")").fadeIn('slow');
        sec = $(this);
        $(this).find('.loadMore').click(function (e) {
            e.preventDefault();
            x16 = (x16 + 6 <= d_size) ? x16 + 6 : d_size;
            $($(this).parent().parent()).find('.movie:lt(' + x16 + ')').fadeIn();
            if (x16 >= d_size)
                $(this).hide();
        });
    });


    // LATEST SHOWS SHOW MORE (Project Air)
    /*var size_li_ls_air = $(".latest_shows_air .movie").size();
    x17=12;
    if(x17 >= size_li_ls_air)		
            $('.latest_shows_air .loadMore').hide();
    $(".latest_shows_air .movie").hide();
    $(".latest_shows_air .movie:lt("+x17+")").fadeIn('slow');
    $('.latest_shows_air .loadMore').click(function (e) {
        e.preventDefault();
        x17= (x17+12 <= size_li_ls_air) ? x17+12 : size_li_ls_air;
        $('.latest_shows_air .movie:lt('+x17+')').fadeIn();
        if(x17 >= size_li_ls_air)		
            $(this).hide();
    });*/

    //Hide loadMore EditProfile
    $('.renewal_subscription_table .loadMore').hide();

    var $cval = $('.input-country #hcountry').val();
    if ($cval != '') $('.input-country #country').val($('ul.dd-country li a[data-value="' + $cval + '"]').text());
    var $sval = $('.input-state #hstate').val();
    if ($sval != '') $('.input-state #state').val($('ul.dd-state li a[data-value="' + $sval + '"]').text());


    if ($('#login_err').length > 0) {
        if ($('.search_box').is(':visible')) {
            $('.search_box').toggle(300);
        }
        $('.login_box').toggle(300);
    }

    /*$('.sign_in_footer_link').click(function () {
        if ($('.search_box').is(':visible'))
            $('.search_box').toggle(300);
        $('.login_box').show(300);
        $('html,body').animate({ scrollTop: 0 }, 'slow');
        return false;
    });*/
	
	$('.register_violator').click(function () {
		dataLayer.push({ 'pageview': '/User/ViolatorRegister', 'pagetitle': 'Registration Attempt via Violator', 'event': 'vpv' });
        return true;
    });

    $("html").niceScroll({ autohidemode: false, cursorwidth: '10px', cursorborderradius: 0, cursorborder: 0, cursorcolor: '#AAAAAB', background: '#F1F1F1', spacebarenabled: false });
});  // end .ready() function

$(window).bind("load", function () {
    setTimeout(function () {
        fixMenu();
    }, 10);
});


$(document.body).on('click', '.dd-state li', function (event) {
    var $target = $(event.currentTarget);
    var $code = $target.find('a').attr('data-value');
    $target.closest('.input-group-btn')
      .parent().find('input#state').val($target.text())
      .parent().find('input#hstate').val($code)
         .end()
      .parent().find('.input-group-btn .dropdown-toggle').dropdown('toggle');
    return false;
});

$(document.body).on('click', '.dd-country li', function (event) {
    var $target = $(event.currentTarget);
    var $code = $target.find('a').attr('data-value');
    $target.closest('.input-group-btn')
      .parent().find('input#country').val($target.text())
      .parent().find('input#hcountry').val($code)
         .end()
      .parent().find('.input-group-btn .dropdown-toggle').dropdown('toggle');
    $.get('/Ajax/GetCountryState', { id: $code }, function () { }).done(function (data) {
        $('.input-state input').val('');
        $('input#city').val('');
        if (data.length > 0) {
            $('.input-state .input-group-btn button').show().parent().find('ul.dropdown-menu').empty();
            $.each(data, function () {
                $('.input-state .input-group-btn ul.dropdown-menu').append('<li><a href="#" data-value="' + this.Value + '">' + this.Text + '</a></li>');
            });
        }
        else { $('.input-state .input-group-btn button').hide(); }
    });
    return false;
});

$(document.body).on("click", "#background_cycler img.bimg", function (e) {
    if ($(this).data("url") != "") {
        location.href = $(this).data("url");
    }
    return false;
});

$(document.body).on("change", "#episodes_select", function (e) {
	dataLayer.push({ 'pageview': '/Episode/EpisodeSelect', 'pagetitle': 'Episode Select via Dropdown', 'event': 'vpv' });
    location.href = "/Episode/Details/" + $(this).val();
    return false;
});

$("#background_cycler .arrows a").hover(
  function () {
      $(this).find("img").attr("src", $(this).find("img").attr("src").replace("out", "in"));
  }, function () {
      $(this).find("img").attr("src", $(this).find("img").attr("src").replace("in", "out"));
  }
);

$(document.body).on("click", "#background_cycler .dots .dott", function (e) {
    cycleImagesSelect($(this).data("ctr"));
    return false;
});

$(document.body).on("click", "#background_cycler a#a_left", function (e) {
    cycleImagesPrev();
    return false;
});
$(document.body).on("click", "#background_cycler a#a_right", function (e) {
    cycleImages();
    return false;
});

function changestate(countryName) {   // change state function body			
    $.get('/Ajax/GetCountryState', { id: countryName }, function (data) { }).done(function (data) {
        if (data.length > 0) {
            var state = '<select class="form-control" id="state" name="state" placeholder="State" required>';
            $.each(data, function () {
                state += '<option value="' + this.Value + '">' + this.Text + '</option>';
            });
            state += '</select>';
            $("#stateCont input").remove();
            $("#stateCont select").remove();
            $("#stateCont").append(state);
        }
        else {
            $("#stateCont input").remove();
            $("#stateCont select").remove();
            $("#stateCont").append('<input type="text" class="form-control" id="state" name="state" placeholder="State" required>');
        }
    });
}

//  [F:16] body for [halo-halo-clicks-videos.html]
function haloHaloClickChangeVideo() {
    var value = $('#hhcv_sortby').val();
    if (value == "Recently Added") {
        $('#hhcv_mviewed').hide();
        $('#hhcv_mshared').hide();
        $('#hhcv_reccently').fadeIn(500);
    } else if (value == "Most Viewed") {
        $('#hhcv_mshared').hide();
        $('#hhcv_reccently').hide();
        $('#hhcv_mviewed').fadeIn(500);
    } else {
        $('#hhcv_reccently').hide();
        $('#hhcv_mviewed').hide();
        $('#hhcv_mshared').fadeIn(500);
    }
}

// [F:17] change background images function body
function cycleImages() {
    var $active = $('#background_cycler img.active');
    var $next = ($('#background_cycler img.active').next('img').length > 0) ? $('#background_cycler img.active').next('img') : $('#background_cycler img:first');
    //find dots
    if ($("#background_cycler .dott").length > 0) {
        var $active_dot = $("#background_cycler .dott img").parent(".active");
        var $next_dot = ($active_dot.next().length > 0) ? $active_dot.next() : $("#background_cycler .dott:first");
    }
    $next.css('z-index', 2);//move the next image up the pile
    $active.fadeOut(1500, function () {//fade out the top image
        //change dot color	
        if ($("#background_cycler .dott").length > 0) {
            $active_dot.find("img").attr("src", $active_dot.find("img").attr("src").replace("white", "gray"));
            $active_dot.removeClass("active");
            $next_dot.find("img").attr("src", $active_dot.find("img").attr("src").replace("gray", "white"));
            $next_dot.addClass("active");
        }
        $active.css('z-index', 1).show().removeClass('active');//reset the z-index and unhide the image		
        $next.css('z-index', 3).addClass('active');//make the next image the top one				
    });
}

function cycleImagesPrev() {
    var $active = $('#background_cycler img.active');
    var $prev = ($('#background_cycler img.active').prev('img').length > 0) ? $('#background_cycler img.active').prev('img') : $('#background_cycler img:first');
    //find dots
    if ($("#background_cycler .dott").length > 0) {
        var $active_dot = $("#background_cycler .dott img").parent(".active");
        var $prev_dot = ($active_dot.prev().length > 0) ? $active_dot.prev() : $("#background_cycler .dott:first");
    }
    $prev.css('z-index', 2);//move the prev image up the pile
    $active.fadeOut(1500, function () {//fade out the top image
        //change dot color	
        if ($("#background_cycler .dott").length > 0) {
            $active_dot.find("img").attr("src", $active_dot.find("img").attr("src").replace("white", "gray"));
            $active_dot.removeClass("active");
            $prev_dot.find("img").attr("src", $active_dot.find("img").attr("src").replace("gray", "white"));
            $prev_dot.addClass("active");
        }
        $active.css('z-index', 1).show().removeClass('active');//reset the z-index and unhide the image		
        $prev.css('z-index', 3).addClass('active');//make the prev image the top one				
    });
}

function cycleImagesSelect(ctr) {
    var $active = $('#background_cycler img.active');
    var $next = ($('#background_cycler img.active').next('img').length > 0) ? $('#background_cycler img.active').next('img') : $('#background_cycler img:first');
    var $selected = ($("#background_cycler img.bimg").eq(ctr).length > 0) ? $("#background_cycler img.bimg").eq(ctr) : $("#background_cycler img:first");
    //find dots
    if ($("#background_cycler .dott").length > 0) {
        var $active_dot = $("#background_cycler .dott img").parent(".active");
        var $next_dot = ($active_dot.next().length > 0) ? $active_dot.next() : $("#background_cycler .dott:first");
        var $selected_dot = ($("#background_cycler .dott").eq(ctr).length > 0) ? $("#background_cycler .dott").eq(ctr) : $("#background_cycler .dott:first");
    }
    $selected.css('z-index', 2);//move the next image up the pile	
    $active.fadeOut(1500, function () {//fade out the top image
        //change dot color	
        if ($("#background_cycler .dott").length > 0) {
            $active_dot.find("img").attr("src", $active_dot.find("img").attr("src").replace("white", "gray"));
            $active_dot.removeClass("active");
            $selected_dot.find("img").attr("src", $active_dot.find("img").attr("src").replace("gray", "white"));
            $selected_dot.addClass("active");
        }
        $active.css('z-index', 1).show().removeClass('active');//reset the z-index and unhide the image		
        $selected.css('z-index', 3).addClass('active');//make the next image the top one				
    });
}

function createCookie(name, value, days) {
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}

var _MS_PER_DAY = 1000 * 60 * 60 * 24;

// a and b are javascript Date objects
function dateDiffInDays(a, b) {
    // Discard the time and time-zone information.
    var utc1 = Date.UTC(a.getFullYear(), a.getMonth(), a.getDate());
    var utc2 = Date.UTC(b.getFullYear(), b.getMonth(), b.getDate());

    return Math.floor((utc2 - utc1) / _MS_PER_DAY);
}

function fixMenu() {		// to fixed main_menu into top
    var lastScrollTop = 0;
    var menu_off = $('.topmenusec').offset();
    var menu_ht = $('.topmenusec').outerHeight();
    menu_off = menu_off.top;

    toSetFixed();
    $(window).bind('scroll', function (e) {
        //$(window).enscroll(function(e) {
        //toSetFixed();
        var st = $(this).scrollTop();
        //alert(st);
        //alert(lastScrollTop);
        if (st > lastScrollTop && lastScrollTop != 0) {
            if ($(window).scrollTop() > (menu_off + menu_ht)) {
                //$('.topmenusec').addClass('fixMenu');
                $('.topmenusec').fadeOut(200, function () {
                    $('body').css('padding-top', menu_ht);
                });

            }
            //$('.topmenusec.fixMenu').fadeOut(200);
        } else {
            if ($(window).scrollTop() <= menu_off) {
                $('.topmenusec').removeClass('fixMenu');
                $('body').css('padding-top', 0);
            } else {
                $('.topmenusec').addClass('fixMenu');
                $('body').css('padding-top', menu_ht);
                $('.topmenusec.fixMenu').fadeIn(200);
            }
        }
        lastScrollTop = st;
    });
    function toSetFixed() {

        if ($(window).scrollTop() > (menu_off + menu_ht)) {
            $('.topmenusec').addClass('fixMenu');
            $('body').css('padding-top', menu_ht);
        }
        else {
            $('.topmenusec').removeClass('fixMenu');
            $('body').css('padding-top', 0);
        }
    }
}