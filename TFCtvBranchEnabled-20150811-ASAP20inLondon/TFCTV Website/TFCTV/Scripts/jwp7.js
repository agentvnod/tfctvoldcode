var jwp;
var streamType = 0;
var advert = null;
var mbrStartingIndex = 0;
var bufferCount = 0;
var minBandwidth = -1;
var maxBandwidth = -1;
var totalBandwidth = 0;
var bwCount = 0;
var completed = false;
var initial = true;
var err = false;
var autostart = false;
var startSeek = false;
var initializeScripts = true;
var _START = 1; var _FINISH = 2; var _STOP = 3; var _PAUSE = 4; var _RESUME = 5;

if (jwpObj.preview || jwpObj.free) {
    advert = { client: 'vast', tag: 'http://ad3.liverail.com/?LR_PUBLISHER_ID=1331&LR_CAMPAIGN_ID=229&LR_SCHEMA=vast2', 'skipoffset': 3 }
}
if (jwpObj.type === "live")
    mbrStartingIndex = 0;

function loadp() {
    $.ajax({
        url: jwpObj.playbackUri
       , dataType: 'json'
    }).done(function (data) {
        if (data.errorCode === 0) {
            initial = true;
            err = false;
            pc(data);
        }
        else if (data.errorCode === -4000) {
            gigya.socialize.logout({
                callback: function (response) {
                    location.href = '/Home/ConcurrentLogin';
                }
            });
            location.href = '/Home/ConcurrentLogin';
        }
        else if (data.errorCode === -704) {
            $(".bs-play .bs-watch").html("Please subscribe to watch this video");
        }
        else {
            $(".bs-play .bs-watch").html("This video is not available");
        }
    });
}

function pc(data) {
    $(function () {
        var uri = data.data.Url;
        var type = uri.split('?').shift().split('.').pop();
        var start = null;
        var end = null;
        var seekToTime = null;
        if (data.data.SubClip != null) {
            start = data.data.SubClip.Start;
            end = data.data.SubClip.End;
        }

        if (jwpObj.lastpos > 0 && jwpObj.mobile) {
            var st = "#t=" + jwpObj.lastpos;
            uri = uri.concat(st);
        }

        jwp = jwplayer("playerTarget");
        jwp.setup(
        {
            playlist:
            [{
                image: jwpObj.ScreenImage,
                file: uri,
                provider: 'http://az332173.vo.msecnd.net/scripts/jwp7/jwplayer.provider.swf',
                type: (type !== "m3u8" ? "mp4" : type)
            }],
            width: "100%",
            height: 366,
            primary: "flash",
            stretching: "uniform",
            abouttext: 'TFC.tv',
            androidhls: true,
            ga: {
                idstring: jwpObj.title,
                label: jwpObj.title
            },
            logo: {
                file: '//az332173.vo.msecnd.net/content/images/logo/player-logo.png',
                hide: true,
                link: 'http://tfc.tv'
            },
            advertising: advert,
            skin: { url: "http://az332173.vo.msecnd.net/scripts/jwp7/skins/glow.css" }
            , flashplayer: "http://az332173.vo.msecnd.net/scripts/jwp7/jwplayer.flash.swf"
            , clipBegin: start
            , clipEnd: end
            , seekToTime: seekToTime
            , wmode: "transparent"
            , mbrStartingIndex: mbrStartingIndex
            //, useMBRStartupBandwidthCheck: 5
        });

        jwp.on("ready", function () {
            //console.log("PLAYER READY");
            $("#playerContainer").css("background", "#000");
            $("#pbtn-silver").attr("src", "http://az332173.vo.msecnd.net/content/images/entrypoint/playbtn-silver.png");
            $(".bs-play .bs-watch").html(jwpObj.ScreenText);
            $("#pbtn-silver").click(function () {
                $(".bs-parental").remove();
                $("#floating_videodiv").fadeOut(800);
                setTimeout(function () { $('.ib_close_bar').fadeOut(800); }, 15000);
                jwp.play();
            });

            $("#watch-later").click(function () {
                jwp.stop();
                if (jwp.getState().toUpperCase() === "IDLE") {
                    $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _STOP, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType, bufferCount: bufferCount, minBandwidth: parseInt(minBandwidth), maxBandwidth: parseInt(maxBandwidth), avgBandwidth: parseInt(totalBandwidth / bwCount) }, function (data) { });
                }
                return false;
            });
        })
        .on("error", function (e) {
            err = true;
            autostart = true;
            console.log(e.message);
            setTimeout(function () { relp(jwpObj.playbackUri); }, 3000);
        })
        .on("seek", function (e) {
            $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _RESUME, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: e.offset, streamType: streamType }, function (data) { });
        })
        .on("play", function (e) {
            if (parseInt(this.getDuration()) === jwpObj.lastpos) {
                startSeek = true;
                jwpObj.lastpos = 0;
            }

            if (initial && jwpObj.lastpos == 0) {
                $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _START, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType }, function (data) { });
                initial = false;
            }
            else
                $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _RESUME, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType }, function (data) { });

            //if (e.oldstate.toUpperCase() !== "BUFFERING") {
            //    if (initial && jwpObj.lastpos == 0) {
            //        $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _START, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType }, function (data) { });
            //        initial = false;
            //    }
            //    else
            //        $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _RESUME, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType }, function (data) { });
            //}

            if (jwpObj.lastpos > 0 && !startSeek) {
                startSeek = true;
                setTimeout(function () { jwp.seek(jwpObj.lastpos); }, 5, true);

                if (jwpObj.mobile) {
                    var video = document.getElementsByTagName("video")[0];
                    video.pause();
                    video.currentTime = jwpObj.lastpos;
                    setTimeout(function () { video.play(); }, 250);
                }
            }

            //initialize the watch later button
            if (initializeScripts) {
                $("#watch-later").addClass("in");
                $(window).unload(function () {
                    $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _STOP, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType, bufferCount: bufferCount, minBandwidth: parseInt(minBandwidth), maxBandwidth: parseInt(maxBandwidth), avgBandwidth: parseInt(totalBandwidth / bwCount) }, function (data) { });
                });
                initializeScripts = false;
            }
        })
        .on("beforePlay", function () {
            if ($("#floating_videodiv").is(":visible")) {
                $(".bs-parental").remove();
                $("#floating_videodiv").fadeOut(800);
                setTimeout(function () { $('.ib_close_bar').fadeOut(800); }, 15000);
            }
        })
        .on("pause", function () {
            $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _PAUSE, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType, bufferCount: bufferCount, minBandwidth: parseInt(minBandwidth), maxBandwidth: parseInt(maxBandwidth), avgBandwidth: parseInt(totalBandwidth / bwCount) }, function (data) { });
        })
        .on("complete", function (e) {
            if (err === false) {
                initial = true;
                setTimeout(function () { $.post('/Ajax/LogPlayback', { type: 1, id: jwpObj.EpisodeId, playTypeId: _FINISH, fullDuration: parseInt(jwp.getDuration()), isPreview: jwpObj.preview, positionDuration: parseInt(jwp.getPosition()), streamType: streamType, bufferCount: bufferCount, minBandwidth: parseInt(minBandwidth), maxBandwidth: parseInt(maxBandwidth), avgBandwidth: parseInt(totalBandwidth / bwCount) }, function (data) { }); }, 1500, true);
                completed = true;
            }
            else
                window.location.reload();
        })
        .on("buffer", function () {
            bufferCount = bufferCount + 1;
        })
        .on("meta", function (e) {
            if (streamType === 0) {
                if (e.metadata.bandwidth) {
                    var bw = e.metadata.bandwidth;
                    if (minBandwidth == -1)
                        minBandwidth = bw;
                    if (bw < minBandwidth)
                        minBandwidth = bw;
                    else if (bw > maxBandwidth)
                        maxBandwidth = bw;
                    totalBandwidth = totalBandwidth + bw;
                    bwCount = bwCount + 1;
                }
            }
        });

        //add useful buttons
        if (!jwpObj.preview && jwpObj.type === "vod") {
            jwp.addButton("http://az332173.vo.msecnd.net/content/images/jwp/icon_ad.png", "Watch Now", function () {
                if (streamType !== 0) {
                    streamType = 0;
                    relp(jwpObj.playbackUri);
                }
            }, "watchnow");
            jwp.addButton("http://az332173.vo.msecnd.net/content/images/jwp/icon_hi.png", "Play High", function () {
                if (streamType !== 1) {
                    streamType = 1;
                    relp(jwpObj.highUri);
                }
            }, "playhigh");
            jwp.addButton("http://az332173.vo.msecnd.net/content/images/jwp/icon_lo.png", "Play Low", function () {
                if (streamType !== 2) {
                    streamType = 2;
                    relp(jwpObj.lowUri);
                }
            }, "playlow");
        }
    });
}

function relp(uri) {
    $.ajax({
        url: uri
        , dataType: 'json'
    }).done(function (data) {
        jwp.stop();
        var u = data.data.Url;
        var tp = u.split('?').shift().split('.').pop();
        initial = true;
        err = false;
        if (streamType === 0) {
            jwp.load([{
                file: u,
                provider: 'http://az332173.vo.msecnd.net/scripts/jwp7/jwplayer.provider.swf',
                type: (tp !== "m3u8" ? "mp4" : tp)
            }]);
        }
        else {
            jwp.load([{
                file: u,
                type: (tp !== "m3u8" || tp === "mp4" ? "mp4" : tp)
            }]);
        }
        jwp.play();
    });
}