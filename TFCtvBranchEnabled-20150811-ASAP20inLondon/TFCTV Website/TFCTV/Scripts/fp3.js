var _START = 1;
var _FINISH = 2;
var _STOP = 3;
var _PAUSE = 4;
var _RESUME = 5;

function Fp(fConfig) {
    this.playerContainer = fConfig.playerContainer;
    this.ajaxUri = fConfig.ajaxUri;
    this.img = fConfig.img;
    this.autoPlay = fConfig.autoPlay;
    this.eId = fConfig.eId;
    this.adUri = fConfig.adUri;
    this.title = fConfig.title;
    this.uri = null;
    this.initial = true;
    this.error = false;
    this.type = null;
    this.completed = false;
    this.bufferCount = 0;
    this.minBandwidth = -1;
    this.maxBandwidth = -1;
    this.totalBandwidth = 0;
    this.bwCount = 0;
    this.logo = "http://az332173.vo.msecnd.net/content/images/logo/player-logo.png";
    this.linkUrl = "http://tfc.tv";
    this.subClip = null;
    this.start = 0;
    this.end = 0;
    this.controls = true;
    this.playbackType = fConfig.playbackType;
    this.smedia = fConfig.smedia;
    this.playLabel = fConfig.playLabel;

    // FLOWPLAYER    
    //this.key = fConfig.key;
    //this.swfUri = fConfig.swfUri;            

    this.showerrors = true;
    this.screenscale = "fit";

    this.preview = fConfig.preview;


    this.GenerateUri = function () {
        return $.ajax({
            url: this.ajaxUri
           , dataType: 'json'
        });
    }

    this.SwitchUri = function (uri) {
        return $.ajax({
            url: uri
           , dataType: 'json'
        });
    }

    this.Initialize = function (data) {
        this.uri = data.data.Url;
        if (data.data.SubClip != undefined) {
            this.subClip = data.data.SubClip;
            this.controls = false;
        }
    }
    this.ShareMedia = function () {
        smedia = this.smedia;
        $.post('/Ajax/ShareMedia', { message: smedia.message, title: smedia.title, description: smedia.description, img: smedia.img, href: smedia.href }, function (data) { });
    }

}

function flowLog(playTypeId, fConfig, fplayer) {
    positionDuration = parseInt(fplayer.getTime());
    fullDuration = parseInt(fplayer.getClip(0).duration);
    $.post('/Ajax/LogPlayback', { type: fConfig.playbackType, id: fConfig.eId, playTypeId: playTypeId, fullDuration: fullDuration, isPreview: fConfig.preview, positionDuration: positionDuration, streamType: 0, bufferCount: 0, minBandwidth: -1, maxBandwidth: -1, avgBandwidth: -1 }, function (data) { });
}

function createfPlayer(fConfig) {
    $.get("/Ajax/MakePlaybackApiRequest/" + fConfig.eId, function (e) {
        if (e.errorCode == "0")
            fConfig.start = e.videoPlayback.Duration != e.videoPlayback.Length ? e.videoPlayback.Duration : 0;

    }).done(function () {
        var urlReady = false;
        flowplayer(fConfig.playerContainer,
        {
            src: "http://az332173.vo.msecnd.net/swf/flowplayer.commercial-hls.swf"
         , wmode: "opaque"
         , version: [10, 1]
         , onFail: function () { console.log("FLASH DID NOT LOAD"); }
         , onLoad: function () { console.log("FLASH READY"); }
        },
        {
            key: "#$9afdd2b2d47848de707"
            , debug: false
        , onLoad: function () {
            console.log("PLAYER LOADED"); this.setVolume(100);
            initializeStreamSense();
            $.post("/Ajax/ResolveMediaUrl", { url: fConfig.uri }, function (data) {
                if (!data.StatusCode) {
                    //ERROR
                }
            });
        }
        , onError: function (e) { }
        , clip:
        {
            url: fConfig.uri
         , provider: "httpstreaming"
         , urlResolvers: "httpstreaming"
         , autoPlay: fConfig.autoPlay
         , scaling: fConfig.screenscale
         , start: fConfig.subClip != null ? fConfig.subClip.Start : (fConfig.start != null ? fConfig.start : 0)
         , duration: fConfig.subClip != null ? fConfig.subClip.End : 0
         , onPause: function () {
             flowLog(_PAUSE, fConfig, this);
             streamSense.notify(ns_.StreamSense.PlayerEvents.PAUSE, { pb_position: flowplayer(fConfig.playerContainer).getTime() }, flowplayer(fConfig.playerContainer).getTime());
         }
            , onResume: function () {
                flowLog(_RESUME, fConfig, this);
                streamSense.notify(ns_.StreamSense.PlayerEvents.PLAY, { pb_position: flowplayer(fConfig.playerContainer).getTime() }, flowplayer(fConfig.playerContainer).getTime());
            }
            , onStop: function () {
                flowLog(_STOP, fConfig, this);
                streamSense.notify(ns_.StreamSense.PlayerEvents.PAUSE, { pb_position: flowplayer(fConfig.playerContainer).getTime() }, flowplayer(fConfig.playerContainer).getTime());
            }
            , onStart: function () {
                flowLog(_START, fConfig, this);
                streamSense.notify(ns_.StreamSense.PlayerEvents.PLAY, { pb_position: flowplayer(fConfig.playerContainer).getTime() }, flowplayer(fConfig.playerContainer).getTime());
            }
            , onFinish: function () {
                flowLog(_FINISH, fConfig, this);
                fConfig.ShareMedia();
                streamSense.notify(ns_.StreamSense.PlayerEvents.END, { pb_position: flowplayer(fConfig.playerContainer).getTime() }, flowplayer(fConfig.playerContainer).getTime());
            }
            , onBufferEmpty: function () {
                streamSense.notify(ns_.StreamSense.PlayerEvents.BUFFER, { pb_position: flowplayer(fConfig.playerContainer).getTime() }, flowplayer(fConfig.playerContainer).getTime());
            }

        }
        , showErrors: false
        , screen:
        {
            width: "100pct"
            , height: "100pct"
        }
        , play:
        {
            replayLabel: "Play again"
            , label: (fConfig.playLabel != null ? fConfig.playLabel : "Watch the full video")
            , fadeSpeed: 500
            , rotateSpeed: 50
            , width: 80
            , height: 80
        }
        , logo:
        {
            url: fConfig.logo
            , fullScreenOnly: false
            , displayTime: 0
            , linkUrl: fConfig.linkUrl
        }
        , contextMenu: [
        {
            "TFC.tv Player 2.0.0.1": function () { location.href = fConfig.linkUrl; }
        }
        ]
        , plugins:
        {
            httpstreaming:
            {
                url: "http://az332173.vo.msecnd.net/swf/flashlsFlowPlayer.swf"
                 , hls_debug: true
                 , hls_debug2: true
                 , hls_startfromlowestlevel: true
            }
            , controls:
            {
                url: "http://az332173.vo.msecnd.net/swf/flowplayer.controls-hls.swf"
                 , backgroundColor: "transparent"
                 , backgroundGradient: "none"
                 , sliderColor: "#FFFFFF"
                 , sliderBorder: "1.5px solid rgba(160,160,160,0.7)"
                 , volumeSliderColor: "#FFFFFF"
                 , volumeBorder: "1.5px solid rgba(160,160,160,0.7)"
                 , timeColor: "#FFFFFF"
                 , durationColor: "#535353"
                 , tooltipColor: "rgba(255, 255, 255, 0.7)"
                 , tooltipTextColor: "#000000"
                 , stop: true
            }
        }
        });
    });
}