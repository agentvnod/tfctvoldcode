/** Development Player **/

flow.Player = function () {
	var obj = {};
	return obj;
}

flow.Player.Create = function (params) {
	var obj = {};
		
	fixVideo = function() {
		var id = params.container + '_api';
		$('#' + id ).attr('src',params.url);
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
					url: params.url
					, ipadUrl: encodeURIComponent(params.ipadUrl)
					, autoPlay: false,
					scaling: params.scaling,
					autoBuffering: false,
					bufferLength: 10,					
					onCuepoint: [
					[300],
					function () {
						console.log('cued');
					}
					],										
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
	}).ipad();
}