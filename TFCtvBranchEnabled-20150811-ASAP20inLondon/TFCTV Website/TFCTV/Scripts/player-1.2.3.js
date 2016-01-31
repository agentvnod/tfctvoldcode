/* File Created: September 16, 2012 */
flow.Player = function () {
    var obj = {};
    return obj;
}

flow.Player.Create = function (params) {
    var obj = {};


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

    flowplayer(params.container, { src: params.playerSWF, wmode: 'opaque', version: [10, 1],
        expressInstall: params.expressInstallSWF,
        onFail: function () {
            //do something here
        }
    },
    {
        key: '#@bfe1f785f0bf6f5b369',
        clip:
                {
                    provider: 'akamai',
                    url: params.url,
                    ipadUrl: params.ipadUrl,
                    autoPlay: false,
                    scaling: params.scaling,
                    autoBuffering: false,
                    bufferLength: 10,
                    onBegin: function () {
                        trackEvent('Videos', 'video-play', buildTrackingLabel(params.label, params.isClip));
                    },
                    onPause: function () { trackEvent('Videos', 'video-pause', buildTrackingLabel(params.label, params.isClip)); },
                    onResume: function () { trackEvent('Videos', 'video-resume', buildTrackingLabel(params.label, params.isClip)); },
                    onStop: function () { trackEvent('Videos', 'video-stop', buildTrackingLabel(params.label, params.isClip)); },
                    onLastSecond: function () {
                        if (params.allowShare)
                            $.post('/Promo/Share', { url: params.userActionURL, id: params.eid });
                        trackEvent('Videos', 'video-finish', buildTrackingLabel(params.label, params.isClip));
                    },
                    eventCategory: params.categoryType
                },
        showErrors: false,
        onError: function (errorCode) {
            //update clip                                       
            if (errorCode == 201) {
                var fplayer = $f(params.container);
                $.ajax({
                    url: params.uri
                    , dataType: 'json'
                    , success: function (data) {
                        if (data.errorCode == 0) {
                            console.log('Updating clip...');
                            var clip = fplayer.getClip();
                            var assetURL = data.data.Url;
                            clip.update({
                                url: assetURL,
                                ipadUrl: assetURL
                            });
                        }
                    }
                    , complete: function () {
                        fplayer.play();
                    }
                });
            }
        },
        screen:
                {
                    height: '100pct',
                    width: '100pct',
                    top: 0
                },
        play:
                {
                    url: params.playButtonURL,
                    replayLabel: 'Play again',
                    fadeSpeed: 500,
                    rotateSpeed: 50,
                    width: 50,
                    height: 50
                },
        logo:
                {
                    url: params.logoURL,
                    fullscreenOnly: false,
                    displayTime: 2000
                },
        contextMenu: ['TFC.tv Video Player v1.1'],
        plugins:
                {
                    gettracker: {
                        url: params.trackerSWF,
                        events: { all: true },
                        accountId: params.trackerID,
                        events: { finish: 'Finish' }
                    },
                    controls:
                        {
                            buttonOffColor: 'rgba(130,130,130,1)',
                            borderRadius: '0px',
                            timeColor: '#ffffff',
                            stop: false,
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
                            url: params.controllerSWF,
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