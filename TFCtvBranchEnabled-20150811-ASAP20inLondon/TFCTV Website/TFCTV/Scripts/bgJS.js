//$(document).ready(function () {
//    $("body").css({ "margin-top": "65px" });
//    setTimeout(function () {
//        $("body").removeClass("subscribed_bg");
//        $("section.page_pagebody").css({ "background": "transparent" });
//        $("body").animate({ opacity: 0 }, 100, function () {
//            $(this)
//                .css({ "background": "url('http://az332173.vo.msecnd.net/content/images/ux/pp30_bg.jpg') top center no-repeat", "background-color": "#000" })
//                .animate({opacity: 1});
//            //$(this)
//            //   .css({ "background": "url('http://az332173.vo.msecnd.net/content/images/ux/pp30_takeover_preorder.jpg') top center no-repeat", "background-color": "#000" })
//            //   .animate({ opacity: 1 });
//        });
//        $("section.page_pagebody,section.topmenusec,section.slidersec,body,html").css({ "cursor": "pointer" });
//        $(".container").css({ "cursor": "default" });
//        $("section.page_pagebody,section.topmenusec,section.slidersec,body,html").click(function (e) {
//            var container = $('.container');
//            if (e.target === this && !container.is(e.target)) {
//                window.open("http://tfc.tv/pinoypride?utm_source=tfctv&utm_medium=banner&utm_content=PP_background&utm_campaign=20150325_PP_background_global", "_blank");
//            }
//        });
//    }, 1000);
//});