/* File Created: February 6, 2012 */
flow.Player = function () {
    var obj = {};
    return obj;
}

flow.Player.Create = function (params) {
    var obj = {};
    reUrl = function (p, params) {
        $.ajax({
            url: '/Episode/_checkvideo/' + params.epId,
            success: function (data) {
                var clip = [{ url: data.data.Url, scale: params.scaling}];
                p.setPlaylist(clip);
            }
        });
    }

    promptSubscription = function (params) {
        if (params.subscribe) {
            $('#featureBanner').append($('#subscribePromptcontainer').html());
        }
    }

    removePrompt = function () {
        $('#featureBanner #subscribePrompt').remove();
    }

    buildTrackingLabel = function (label, clip) {
        if (clip)
            label += '|Preview';
        else
            label += '|Full';
        return label;
    }

    trackEvent = function (category, action, label) {
        _gaq.push(['t2._trackEvent', category, action, label]);
    }

    flowplayer(params.container, { src: params.swf, wmode: 'opaque', version: [10, 1],
        expressInstall: 'http://cdnassets.tfc.tv/player/expressInstall.swf',
        onFail: function () {
            $('#' + params.container).html($('#getFlash').html());
            $('#goGetFlash').click(function () { window.open($(this).attr('href'), '_newtab'); return false; });
        }
    },
    {
        //onLoad: function () { reUrl(this, params); },
        //log: { level: 'debug' },
        key: '#@bfe1f785f0bf6f5b369',
        clip:
                {
                    provider: 'akamai',
                    ipadUrl: params.ipad,
                    url: params.url,
                    autoPlay: params.autoPlay == undefined ? false : params.autoPlay,
                    scaling: params.scaling,
                    bufferLength: 10,
                    autoBuffering: params.autoPlay == undefined ? false : params.autoPlay,
                    onStart: function () {
                        lightsOut(); removePrompt();
                        trackEvent('Videos', 'video-play', buildTrackingLabel(params.label, params.isClip));
                        this.getPlugin('play').css({ opacity: 0.3 });

                    },
                    stopLiveOnPause: false,
                    onBufferEmpty: function () { this.getPlugin('play').css({ opacity: 0.3 }); },
                    onPause: function () { lightsOn(); trackEvent('Videos', 'video-pause', buildTrackingLabel(params.label, params.isClip)); this.getPlugin('play').css({ opacity: 0.8 }); },
                    //onStart: function () { var p = this; console.log('onStart: ' + p.getClip().completeUrl); },
                    //onBeforePause: function (clip) { var p = this; console.log(p.getClip().completeUrl); },
                    onResume: function () { lightsOut(); trackEvent('Videos', 'video-resume', buildTrackingLabel(params.label, params.isClip)); this.getPlugin('play').css({ opacity: 0.3 }); },
                    onStop: function () { /*reUrl(this, params);*/lightsOn(); trackEvent('Videos', 'video-stop', buildTrackingLabel(params.label, params.isClip)); this.getPlugin('play').css({ opacity: 0.8 }); },
                    onFinish: function () {
                        //reUrl(this, params);
                        lightsOn();
                        promptSubscription(params);
                        $.post('/Ajax/ShareEpisode/' + params.epId, { type: 'watch', isClip: params.isClip, categoryType: params.categoryType, sid: params.sid });
                        trackEvent('Videos', 'video-finish', buildTrackingLabel(params.label, params.isClip));
                        this.getPlugin('play').css({ opacity: 0.8 });
                    },
                    eventCategory: params.categoryType
                },
        screen:
                {
                    height: '100pct',
                    width: '940px',
                    top: 0
                },
        play:
                {
                    url: 'http://cdnassets.tfc.tv/content/images/logo/play-button.png',
                    replayLabel: 'Play again',
                    fadeSpeed: 500,
                    rotateSpeed: 50,
                    width: 50,
                    height: 50,
                    opacity: 0.8
                },
        logo:
                {
                    url: 'http://cdnassets.tfc.tv/content/images/logo/player-logo.png',
                    fullscreenOnly: false,
                    displayTime: 2000
                },
        contextMenu: ['TFC.tv Video Player v1.0'],
        plugins:
                {
                    gettracker: {
                        url: 'http://cdnassets.tfc.tv/swf/flowplayer.analytics.swf',
                        events: { all: true },
                        accountId: 'UA-2265816-2',
                        events: { finish: 'Finish' }
                    },
                    controls:
                        {
                            buttonOffColor: 'rgba(130,130,130,1)',
                            borderRadius: '0px',
                            timeColor: '#ffffff',
                            stop: true,
                            bufferGradient: 'none',
                            sliderColor: '#000000',
                            zIndex: 45,
                            backgroundColor: '#111000',
                            scrubberHeightRatio: 0.6,
                            volumeSliderGradient: 'none',
                            tooltipTextColor: '#ffffff',
                            sliderGradient: 'none',
                            spacing:
                                {
                                    time: 6,
                                    volume: 8,
                                    all: 2
                                },
                            timeBorderRadius: 20,
                            timeBgHeightRatio: 0.8,
                            volumeSliderHeightRatio: 0.6,
                            progressGradient: 'none',
                            height: 26,
                            time: params.time,
                            volumeColor: '#4599ff',
                            tooltips:
                                {
                                    marginBottom: 5,
                                    buttons: false
                                },
                            timeSeparator: ' ',
                            name: 'controls',
                            volumeBarHeightRatio: 0.2,
                            opacity: 1,
                            timeFontSize: 12,
                            left: '50pct',
                            tooltipColor: 'rgba(0, 0, 0, 0)',
                            volumeSliderColor: '#ffffff',
                            border: '0px',
                            bufferColor: '#a3a3a3',
                            buttonColor: '#ffffff',
                            durationColor: '#b8d9ff',
                            autoHide:
                                {
                                    enabled: true,
                                    hideDelay: 500,
                                    hideStyle: 'fade',
                                    mouseOutDelay: 500,
                                    hideDuration: 400,
                                    fullscreenOnly: false
                                },
                            backgroundGradient: 'none',
                            width: '100pct',
                            sliderBorder: '1px solid rgba(128, 128, 128, 0.7)',
                            display: 'block',
                            buttonOverColor: '#ffffff',
                            url: 'http://cdnassets.tfc.tv/player/flowplayer.controls-v3.2.5.swf',
                            timeBorder: '0px solid rgba(0, 0, 0, 0.3)',
                            progressColor: '#4599ff',
                            timeBgColor: 'rgb(0, 0, 0, 0)',
                            scrubberBarHeightRatio: 0.2,
                            bottom: 0,
                            builtIn: false,
                            volumeBorder: '1px solid rgba(128, 128, 128, 0.7)',
                            margins: [2, 12, 2, 12]
                        }
                    , akamai: params.apObj
                }
    }).ipad();
}