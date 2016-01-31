var Freetv = {

    initialSmil: '',

    init: function () {
        //$('#quickJump').customStyle();

        Freetv.player = $('#player');
        if (Freetv.initialSmil != '') {
            Freetv.loadVideo(Freetv.initialSmil);
        }

        $('a.related-clip, a.related-clip-link').click(function (e) {

            e.preventDefault();
            e.stopPropagation();

            var element = $(this);
            var smil = element.attr('href');

            var isplit = smil.split('#');

            if ($(isplit[1]).defined()) {
                var vididsplit = isplit[1].split('_');
                var videoid = vididsplit[1];

                smil = isplit[0] + '#' + vididsplit[0];

                // Fetch video details here
                Global.ajaxBasicGet({
                    url: '/ajax/handler',
                    type: 'json',
                    preload: function () {
                        //reviewDiv.find('span.loading').show();
                    },
                    data: {
                        'method': 'getVideo',
                        'vid': videoid
                    },
                    callback: function (params, data) {

                        //reviewDiv.find('span.loading').hide();

                        if (data.status == "success") {
                            $('#now-playing-title').html(data.video.title);
                            $('#now-playing-info').html(data.video.shortInfo);
                            $('#now-playing-date').html(data.video.airingDate);
                            $('#now-playing-views').html(data.video.viewedNum);
                        }
                    }
                });
            }

            Freetv.loadVideo(smil);

            $('a.related-clip').removeClass('now-playing').removeClass('up-next');
            element.addClass('now-playing');
            element.next('a.related-clip').addClass('up-next');

        });

        /**Free TV menu**/
        $("#freetv-navigation li").click(function (event) {
            var clickedID = $(this).attr("id");

            if (clickedID != "freetv-logo") {
                $("#freetv-navigation li").removeClass("selected");
                $("#" + clickedID).addClass("selected");

                event.preventDefault();
            }
        });
        /****/

        $('#freetv-tabs li').click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            $("ul#freetv-tabs li").removeClass("current");
            $(this).addClass("current");
        });
        $('#comedy-tab').click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            $('.freetv-contents').hide();
            $('#comedy-container').show();
        });
        $('#drama-tab').click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            $('.freetv-contents').hide();
            $('#drama-container').show();
        });
        $('#sports-tab').click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            $('.freetv-contents').hide();
            $('#sports-container').show();
        });
        /*
        $('#trailertab').click(function(e) {
        e.preventDefault();
        e.stopPropagation();
			
        $('.freetv_contents').hide();
        $('#trailercontainer').show();
        });
        */

        //*******	$('#nltextbox').placeholder('Search Free TV...');

        $('#quickJump').change(function () {
            var element = $(this);
            var selected = element.find('option:selected');

            var magulang = element.parents('div.topchart');

            location.href = '/freetv/' + magulang.attr('id') + '/' + selected.val();
        })
    },

    nextVideo: function (vid) {

        // Fetch video details here
        Global.ajaxBasicGet({
            url: '/ajax/handler',
            type: 'json',
            preload: function () {
                //reviewDiv.find('span.loading').show();
            },
            data: {
                'method': 'getVideo',
                'vid': vid
            },
            callback: function (params, data) {

                //reviewDiv.find('span.loading').hide();

                if (data.status == "success") {
                    $('#now-playing-title').html(data.video.title);
                    $('#now-playing-info').html(data.video.shortInfo);
                    $('#now-playing-date').html(data.video.airingDate);
                    $('#now-playing-views').html(data.video.viewedNum);
                }
            }
        });

        var element = $('#related-clip_' + vid);
        $('a.related-clip').removeClass('now-playing').removeClass('up-next');
        element.addClass('now-playing');

        //element.next('a.related-clip').addClass('up-next');
        element.parent('div').next('div').find('a.related-clip').addClass('up-next');
    },

    loadVideo: function (smilURL) {

        // Check if #flashContent exists
        if (!$('#flashContent').exists()) {
            // It doesn't exist! 

            // Clear the contents of the player div
            Freetv.player.html('');

            // Put the #flashContent div inside
            $('<div>').attr('id', 'flashContent').appendTo(Freetv.player);
        }

        // Parse smilURL for index
        var index = 1;
        var isplit = smilURL.split('#');
        if ($(isplit[1]).defined()) {
            smilURL = isplit[0];
            index = isplit[1];
        }

        //<!-- Adobe recommends that developers use SWFObject2 for Flash Player detection. -->
        //<!-- For more information see the SWFObject page at Google code (http://code.google.com/p/swfobject/). -->
        // <!-- Information is also available on the Adobe Developer Connection Under Detecting Flash Player versions and embedding SWF files with SWFObject 2" -->
        // <!-- Set to minimum required Flash Player version or 0 for no version detection -->
        var swfVersionStr = "10.1.52";
        // <!-- xiSwfUrlStr can be used to define an express installer SWF. -->
        var xiSwfUrlStr = "";
        var flashvars = {};
        flashvars.src = smilURL;
        flashvars.trackNo = index;
        flashvars.enableLogging = 0;
        flashvars.enableToken = 1;
        //flashvars.isLoggedIn = Global.loggedIn;    	
        var params = {};
        params.quality = "high";
        params.bgcolor = "#000000";
        params.play = "true";
        params.loop = "true";
        params.wmode = "transparent";
        params.scale = "showall";
        params.menu = "true";
        params.devicefont = "false";
        params.salign = "";
        params.allowscriptaccess = "sameDomain";
        params.allowFullScreen = "true";
        var attributes = {};
        attributes.id = "TFCPlayer";
        attributes.name = "TFCPlayer";
        attributes.align = "middle";

        //var swfFile = "/swf/TFCPlayer.swf";
        var swfFile = "/swf/Player.swf";
        if (smilURL.substring(smilURL.length - 3, smilURL.length) == "xml") {
            //swfFile = "/swf/TFCPlayer.swf";
            swfFile = "/swf/Player.swf";
        }
        /*
        swfobject.createCSS("html", "height:100%; background-color: #ffffff;");
        swfobject.createCSS("body", "margin:0; padding:0; overflow:hidden; height:100%;");
	    
        $(document).ready(function(){
        });
        */
        swfobject.embedSWF(
	        swfFile, "flashContent",
        //"940", "483",
        //"940", "406",
	        "540", "406",
	        swfVersionStr, xiSwfUrlStr,
	        flashvars, params, attributes
        );
    }
}

$(document).ready(function () {
    Freetv.init();
});