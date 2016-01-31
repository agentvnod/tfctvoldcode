var _START = 1;
var _FINISH = 2;
var _STOP = 3;
var _PAUSE = 4;
var _RESUME = 5;

function Jwp(jConfig) {
    this.playerContainer = jConfig.playerContainer;
    this.ajaxUri = jConfig.ajaxUri;
    this.img = jConfig.img;
    this.autoPlay = jConfig.autoPlay;
    this.eId = jConfig.eId;
    this.adUri = jConfig.adUri;
    this.title = jConfig.title;
    this.uri = null;
    this.aspectratio = "16:9";
    this.width = "100%";
    this.height = jConfig.height;
    this.stretching = "uniform";
    this.aboutText = "TFC.tv";
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
    this.logo_link = "http://tfc.tv";
    this.subClip = null;
    this.start = 0;
    this.end = 0;
    this.controls = true;
    this.playbackType = jConfig.playbackType;
    this.smedia = jConfig.smedia;

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

    this.CreateObject = function () {
        initial = this.initial;
        error = this.error;
        completed = this.completed;
        this.type = this.uri.split('?').shift().split('.').pop();
        start = 0; end = 0; seek = false; preview = false;
        if (this.subClip != undefined) {
            start = this.subClip.Start;
            end = this.subClip.End;
            preview = true;
        }

        playerContainer = this.playerContainer;
        adUri = this.adUri;
        eId = this.eId;

        bufferCount = this.bufferCount;
        minBandwidth = this.minBandwidth;
        maxBandwidth = this.maxBandwidth;
        totalBandwidth = this.totalBandwidth;
        bwCount = this.bwCount;
        playbackType = this.playbackType;

        return jwplayer(this.playerContainer).setup({
            file: this.uri,
            controls: this.controls,
            image: this.img,
            width: this.width,
            height: this.height,
            aspectratio: (this.height == null ? this.aspectratio : null),
            stretching: this.stretching,
            type: this.type,
            primary: "flash",
            abouttext: this.aboutText,
            androidhls: true,
            autostart: this.autoPlay,
            autoplay: this.autoPlay,
            logo: {
                file: this.logo,
                link: this.logo_link
            },
            ga: {},
            advertising: {
                client: "vast",
                companiondiv: { id: "adrectangle", height: 250, width: 300 },
                skipoffset: 5,
                admessage: 'Your video will resume in XX seconds...'
            }
        })
        .onPlay(function (e) {
            if (initial) {
                $.post('/Ajax/LogPlayback', { type: playbackType, id: eId, playTypeId: _START, fullDuration: jwplayer(playerContainer).getDuration(), isPreview: preview, positionDuration: jwplayer(playerContainer).getDuration(), streamType: 0 }, function (data) { });
                if (!error) {
                    setTimeout(function () {
                        jwplayer(playerContainer).setCurrentQuality(0);
                    }, 1000);
                }
                initial = false;
            }
            else
                $.post('/Ajax/LogPlayback', { type: playbackType, id: eId, playTypeId: _RESUME, fullDuration: jwplayer(playerContainer).getDuration(), isPreview: preview, positionDuration: jwplayer(playerContainer).getDuration(), streamType: 0 }, function (data) { });
            if (start > 0) {
                if (!seek) {
                    jwplayer(playerContainer).seek(start);
                    seek = true;
                }
            }
        })
        .onPause(function (e) {
            initial = true;
            $.post('/Ajax/LogPlayback', { type: playbackType, id: eId, playTypeId: _PAUSE, fullDuration: jwplayer(playerContainer).getDuration(), isPreview: preview, positionDuration: jwplayer(playerContainer).getDuration(), streamType: 0, bufferCount: bufferCount, minBandwidth: minBandwidth, maxBandwidth: maxBandwidth, avgBandwidth: parseInt(totalBandwidth / bwCount) }, function (data) { });
        })
        .onComplete(function (e) {
            if (!error) {
                initial = true;
                $.post('/Ajax/LogPlayback', { type: playbackType, id: eId, playTypeId: _FINISH, fullDuration: jwplayer(playerContainer).getDuration(), isPreview: preview, positionDuration: jwplayer(playerContainer).getDuration(), streamType: 0, bufferCount: bufferCount, minBandwidth: minBandwidth, maxBandwidth: maxBandwidth, avgBandwidth: parseInt(totalBandwidth / bwCount) }, function (data) { });
                completed = true;
            }
            else
                window.location.reload();
        })
        .onBuffer(function (e) {
            bufferCount = bufferCount + 1;
        })
        .onMeta(function (e) {
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
        })
        .onBeforePlay(function () {
            if (adUri != null || adUri != undefined)
                jwplayer(playerContainer).playAd(adUri);
        })
        .onError(function (e) {
            error = true;
        })
        .onTime(function (e) {
            if (end > 0) {
                if (e.position >= end)
                    jwplayer(playerContainer).stop();
            }
        })
        .onSeek(function (e) { });
    };
}