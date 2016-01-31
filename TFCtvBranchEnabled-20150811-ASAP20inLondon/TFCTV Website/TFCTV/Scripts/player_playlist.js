/* File Created: February 6, 2012 */
flow.Player = function () {
    var obj = {};
    return obj;
}

flow.Player.Create = function (params) {
    var obj = {};
    flowplayer(params.container, { src: params.swf, wmode: 'opaque' },
    {
        clip:
                {
                    provider: 'akamai',
                    url: params.url,
                    autoPlay: false,
                    scaling: params.scaling,
                    autoBuffering: false,
                    onStart: lightsOut,
                    onPause: lightsOn,
                    onResume: lightsOut,
                    onStop: lightsOn,
                    onFinish: function () {
                        lightsOn();
                        $.post('/Ajax/PublishAction/' + params.epId, { type: 'watch' });
                    },
                    onBegin: lightsOut
                },
        screen:
                {
                    height: '100pct',
                    width: '940px',
                    top: 0
                },
        play:
                {
                    url: '/Content/images/logo/play-button.png',
                    replayLabel: 'Play again',
                    fadeSpeed: 500,
                    rotateSpeed: 50
                },
        plugins:
                {
                    controls:
                        {
                            buttonOffColor: 'rgba(130,130,130,1)',
                            borderRadius: '0px',
                            timeColor: '#ffffff',
                            stop: true,
                            bufferGradient: 'none',
                            sliderColor: '#000000',
                            zIndex: 45,
                            backgroundColor: 'rgba(0, 0, 0, 0)',
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
                            time: false,
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
                            url: '/Player/flowplayer.controls-v3.2.5.swf',
                            timeBorder: '0px solid rgba(0, 0, 0, 0.3)',
                            progressColor: '#4599ff',
                            timeBgColor: 'rgb(0, 0, 0, 0)',
                            scrubberBarHeightRatio: 0.2,
                            bottom: 0,
                            builtIn: false,
                            volumeBorder: '1px solid rgba(128, 128, 128, 0.7)',
                            margins: [2, 12, 2, 12]
                        },
                    akamai:
                        {
                            url: '/Player/AkamaiFlowPlugin.swf',
                            subClip: params.apObj
                        }
                }
    });
}