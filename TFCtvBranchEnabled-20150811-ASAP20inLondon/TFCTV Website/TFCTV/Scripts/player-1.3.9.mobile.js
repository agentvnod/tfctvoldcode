/* File Created: September 16, 2012 */

var vSTART = 1;
var vFINISH = 2;
var vSTOP = 3;
var vPAUSE = 4;
var vRESUME = 5;

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

	logPlayback = function (type, id, playTypeId, fullDuration, isPreview, positionDuration, streamType) {
		$.post('/Ajax/LogPlayback', { type: type, id: id, playTypeId: playTypeId, fullDuration: Math.round(fullDuration), isPreview: isPreview, positionDuration: Math.round(positionDuration), streamType: streamType }, function (data) { console.log(data.errorCode); });
	}
	
	promptSubscription = function (params) {
		if (params.subscribe) {
			$('#featureBanner').append($('#subscribePromptcontainer_1').html());
		}
	}
	
	fixVideo = function() {
		var id = params.container + '_api';
		$('#' + id ).attr('src',params.url);
	}
		
	flowplayer(params.container, { src: params.playerSWF, wmode: 'opaque', version: [10, 1],
		expressInstall: params.expressInstallSWF,
		onFail: function () {
			//do something here
		},
		onLoad: function() {
			//increase volume setting
			this.setVolume(100);
		}
	},
	{
		key: '#@bfe1f785f0bf6f5b369',
		clip:
				{
					provider: 'akamai',
					url: params.url
					, ipadUrl: encodeURIComponent(params.ipadUrl)
					, autoPlay: false,
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
							$.post('/Ajax/ShareMedia' , { message: params.gmessage, title: params.gtitle, description: params.gdescription, img: params.gimg, href: params.ghref });
						trackEvent('Videos', 'video-finish', buildTrackingLabel(params.label, params.isClip));
					},
					onCuepoint: [
					[300],
					function () {
						logPlayback(params.playbackType, params.playbackId, vSTART, this.getClip().duration, params.isClip, this.getTime(), params.streamType);
						console.log('cued');
					}
					],
					onStart: function () {
						this.setVolume(100);
						if (params.playbackType == 2 || params.playbackType == 4)
							logPlayback(params.playbackType, params.playbackId, vSTART, this.getClip().duration, params.isClip, this.getTime(), params.streamType);
					},
					onBeforePause: function () {
						logPlayback(params.playbackType, params.playbackId, vPAUSE, this.getClip().duration, params.isClip, this.getTime(), params.streamType);
					},
					onBeforeResume: function () {
						logPlayback(params.playbackType, params.playbackId, vRESUME, this.getClip().duration, params.isClip, this.getTime(), params.streamType);
					},
					onBeforeStop: function () {
						logPlayback(params.playbackType, params.playbackId, vSTOP, this.getClip().duration, params.isClip, this.getTime(), params.streamType);
					},
					onBeforeFinish: function () {
						logPlayback(params.playbackType, params.playbackId, vFINISH, this.getClip().duration, params.isClip, this.getTime(), params.streamType);
					},
					onFinish: function () {                                              
						promptSubscription(params);                        
					},
					eventCategory: params.categoryType
				},
		showErrors: false,
		onLoad: function() {
			console.log('Loading TFC.tv player...');
			var deviceAgent = navigator.userAgent.toLowerCase();
			var agentID = deviceAgent.match(/(iPad|iPhone|iPod)/i);
			if (agentID)
				setTimeout('fixVideo()',150);
		},
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
								url: assetURL
								, ipadUrl: encodeURIComponent(assetURL)
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
					controls: {
						  borderRadius: '0',
						  durationColor: '#a3a3a3',
						  tooltipTextColor: '#ffffff',
						  sliderColor: '#000000',
						  timeSeparator: ' ',
						  tooltipColor: '#000000',
						  volumeSliderColor: '#ffffff',
						  bufferGradient: 'none',
						  volumeSliderGradient: 'none',
						  timeBgColor: 'rgb(0, 0, 0, 0)',
						  buttonColor: '#ffffff',
						  disabledWidgetColor: '#555555',
						  backgroundColor: '#000000',
						  timeColor: '#ffffff',
						  buttonOverColor: '#ffffff',
						  sliderBorder: '1px solid rgba(128, 128, 128, 0.7)',
						  callType: 'default',
						  volumeBorder: '1px solid rgba(128, 128, 128, 0.7)',
						  bufferColor: '#b4a8a7',
						  backgroundGradient: 'none',
						  sliderGradient: 'none',
						  buttonOffColor: 'rgba(130,130,130,1)',
						  progressColor: '#de5349',
						  autoHide: {
								enabled: true,
								hideDelay: 500,
								hideStyle: 'fade',
								mouseOutDelay: 500,
								hideDuration: 400,
								fullscreenOnly: false
						  },
						  volumeColor: '#ffffff',
						  progressGradient: 'none',
						  timeBorder: '0px solid rgba(0, 0, 0, 0.3)',
						  height: 24,
						  opacity: 1.0,
						  stop: false,
						  zIndex: 45,
						  time: params.time,
						  url: params.controllerSWF,
						  scrubber: true,
						  stop: true
					   }
					, akamai: params.apObj
					, akamaiAnalytics: {
						url: params.analyticsSWF,
						csmaPluginPath: "http://79423.analytics.edgesuite.net/csma/plugin/csma.swf",
						csmaConfigPath: "http://ma231-r.analytics.edgesuite.net/config/beacon-4012.xml"

					}
				}
	}).ipad({ simulateiDevice: true, controls: false });
}