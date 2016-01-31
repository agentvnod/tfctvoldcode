/* Initialize */
tfc.Social = function () {
    var obj = {};
        obj.onConnect = function (eventObj) {
            var url = '/Social/Login?retUrl=' + encodeURIComponent(location.href) + '&UID=' + encodeURIComponent(eventObj.UID) + '&UIDSig=' + eventObj.UIDSig + '&UIDSignature=' + eventObj.UIDSignature + '&signature=' + encodeURIComponent(eventObj.signature) + '&timestamp=' + encodeURIComponent(eventObj.timestamp) + '&provider=' + eventObj.provider + '&isSiteUID=' + eventObj.user.isSiteUID + '&email=' + encodeURIComponent(eventObj.user.email) + '&firstName=' + encodeURIComponent(eventObj.user.firstName) + '&lastName=' + encodeURIComponent(eventObj.user.lastName) + '&country=' + eventObj.user.country;
            setTimeout('window.location.href = "' + url + '";', 800);
            console.log('connect: ' + new Date());
        }

        obj.onLogout = function (eventObj) {
            location.reload(true);
        }

        obj.onConnectionAdded = function (eventObj) {
        }
        gigya.socialize.addEventHandlers({ onConnectionAdded: obj.onConnectionAdded, onLogout: obj.onLogout, onLogin: obj.onConnect });
    return obj;
}

tfc.Social.EditConnectionsUI = function (params) {
    var obj = {};
    gigya.socialize.showEditConnectionsUI({
        containerID: params.container,
        width: params.width,
        height: params.height,
        showTermsLink: false,
        hideGigyaLink: true,
        enabledProviders: params.enabledProviders
    });
}

tfc.Social.ShowActivityFeed = function (params) {
    var obj = {};
    gigya.socialize.showFeedUI({
        containerID: params.container,
        width: '500'
    });
}

tfc.Social.ChatUI = function (params) {
    var obj = {};
    
    obj.onConnect = function (eventObj) {
        var url = '/Social/Login?retUrl=' + encodeURIComponent(location.href) + '&UID=' + encodeURIComponent(eventObj.UID) + '&UIDSig=' + eventObj.UIDSig + '&UIDSignature=' + eventObj.UIDSignature + '&signature=' + encodeURIComponent(eventObj.signature) + '&timestamp=' + encodeURIComponent(eventObj.timestamp) + '&provider=' + eventObj.provider + '&isSiteUID=' + eventObj.user.isSiteUID + '&email=' + encodeURIComponent(eventObj.user.email) + '&firstName=' + encodeURIComponent(eventObj.user.firstName) + '&lastName=' + encodeURIComponent(eventObj.user.lastName) + '&country=' + eventObj.user.country;
        setTimeout('window.location.href = "' + url + '";', 800);
        console.log('connect: ' + new Date());
    }

    obj.onLogout = function (eventObj) {
        window.location.href = '/User/LogOut';
    }

    //gigya.socialize.addEventHandlers({ onLogout: obj.onLogout, onLogin: obj.onConnect });
    gigya.socialize.showChatUI(params);
}

tfc.Social.PublishUserAction = function (params) {
    var obj = {};

    obj.initUserAction = function () {
        act = new gigya.socialize.UserAction();
        act.setUserMessage(action.message);
        act.setTitle(action.title);
        act.setLinkBack(action.linkback);
        act.setDescription(action.description);
        act.addActionLink(action.actionlinkmessage, action.actionlinkurl);
        act.addMediaItem({ type: action.mediaItemImage, src: action.mediaItemSrc, href: action.mediaItemHref });
        return act;
    }

    var action = { userAction: obj.initUserAction() };
    gigya.socialize.publishUserAction(action);
}

tfc.Social.Share = function (params, action) {
    var obj = {};
    var act = {};
    var shareButtons = 'share';

    obj.onShare = function (event) {
        alert('You are not logged in.');
        event.preventDefault();
    }

    onLoadShareButtonHandler = function () {
        if (params.isLogin == 'false')
            $(".gig-share-button").attr("onclick", "");
    }

    onSharedDoneHandler = function (responseObj) {
        /****/
        var actInternal = new gigya.socialize.UserAction();
        actInternal.setLinkBack(action.linkback);
        actInternal.setTitle(action.title);
        actInternal.setDescription('has shared ');
        actInternal.addMediaItem(action.mediaitem);
        actInternal.actorUID = action.actorUID;

        var params = {
            feedID: 'UserAction',
            userAction: actInternal,   // including the UserAction object
            scope: 'internal', // the Activity Feed will be published internally only (site scope), not to social networks
            privacy: 'public'//,
            //callback: publishInternalUserAction_callback
        };

        gigya.socialize.publishUserAction(params);
    }

    obj.initUserAction = function () {
        act = new gigya.socialize.UserAction();
        act.setTitle(action.title);
        act.setSubtitle("www.tfc.tv");
        act.setLinkBack(action.linkback);
        act.setDescription(action.description);
        act.addActionLink(action.actionlinkmessage, action.actionlinkurl);
        act.addMediaItem(action.mediaitem);
    }

    obj.initShareBar = function () {
        if (params.shareButtons != null)
            shareButtons = params.shareButtons;
        gigya.socialize.showShareBarUI({
            containerID: params.container,
            shareButtons: shareButtons,
            showCounts: params.showCounts,
            grayedOutScreenOpacity: 20,
            userAction: act,
            shortURLs: 'whenRequired',
            operationMode: 'autoDetect',
            onLoad: onLoadShareButtonHandler,
            onSendDone: onSharedDoneHandler
        });
    }

    obj.initUserAction();
    obj.initShareBar();
    return obj;
}

tfc.Social.Login = function (params) {
    var obj = {};

    //onLogin handler
    //    obj.onLoginHandler = function (eventObj) {
    //        alert(eventObj.UID);
    //    }
    var UIConfig = '';
    if (params.UIConfig == true) {
        UIConfig = '<config><body><controls><snbuttons buttonsize="' + params.buttonSize + '" /></controls></body></config>';
    }

    var lastLoginIndication = 'border';
    if (params.lastLoginIndication != undefined || params.lastLoginIndication != null) {
        lastLoginIndication = params.lastLoginIndication;
    }

    obj.initLogin = function () {
        gigya.socialize.showLoginUI({
            containerID: params.container
        , width: params.width
        , height: params.height
        , buttonsStyle: params.style
        , enabledProviders: params.enabledProviders
            //, redirectURL: '/Social/Login?returl=' + location.href
        , showTermsLink: false
        , UIConfig: UIConfig
        , hideGigyaLink: true
        , lastLoginIndication: lastLoginIndication
        , lastLoginButtonSize: 25
        });
    }

    obj.initShowAddConnections = function () {
        gigya.socialize.showAddConnectionsUI({
            containerID: params.container
        , width: params.width
        , height: params.height
        , enabledProviders: params.enabledProviders
        , buttonsStyle: params.style
        , showTermsLink: false
        , hideGigyaLink: true
        , showEditLink: false
        , sessionExpiration: 0
        , cid: params.cid
        });
    }

    obj.onConnect = function (eventObj) {
        var url = '/Social/Login?retUrl=' + encodeURIComponent(location.href) + '&UID=' + encodeURIComponent(eventObj.UID) + '&UIDSig=' + eventObj.UIDSig + '&UIDSignature=' + eventObj.UIDSignature + '&signature=' + encodeURIComponent(eventObj.signature) + '&timestamp=' + encodeURIComponent(eventObj.timestamp) + '&provider=' + eventObj.provider + '&isSiteUID=' + eventObj.user.isSiteUID + '&email=' + encodeURIComponent(eventObj.user.email) + '&firstName=' + encodeURIComponent(eventObj.user.firstName) + '&lastName=' + encodeURIComponent(eventObj.user.lastName) + '&country=' + eventObj.user.country;
        setTimeout('window.location.href = "' + url + '";', 800);
        console.log('connect: ' + new Date());
    }

    obj.onLogout = function (eventObj) {
        window.location.href = '/User/LogOut';
    }

    obj.onConnectionAdded = function (eventObj) {
        //alert('You have just added a new social network to your account!');
    }

    if (params.isLogin == undefined || params.isLogin == false) {
        obj.initLogin();
        //gigya.socialize.addEventHandlers({ onLogout: obj.onLogout, onLogin: obj.onConnect });
    }
    else {
        obj.initShowAddConnections();
        //gigya.socialize.addEventHandlers({ onConnectionAdded: obj.onConnectionAdded, onLogout: obj.onLogout });
    }
    //if (params.addHandler)
    return obj;
    //gigya.socialize.addEventHandlers({ onLogin: obj.onLoginHandler });
}

/**Gigya Social variables**/
var _COMMENT = 1;
var _RATINGSANDREVIEW = 2;
var _SHARE = 3;

/*-=Reactions=-*/
var _LIKE = 11;
var _LOVE = 12;

tfc.Social.getEngagementId = function (param) {
    if (param == 'like')
        return _LIKE;
    if (param == 'love')
        return _LOVE;
    if (param == 'share')
        return _SHARE;
    if (param == 'rate')
        return _RATINGSANDREVIEW;
    if (param == 'comment')
        return _COMMENT;

    return 0; //_NOTFOUND
}

tfc.Social.OnSiteLogin = function () {
    $('#signin').click();
    return false;
}

tfc.Social.OnRatingsAndReviews = function (params) {
    if (params.rated == 'True') {
        $("#commentsDiv-postButton a").addClass("gig-comments-button-disabled").addClass("gig-comments-button-post-disabled").addClass("gig-comments-commentBox-button-post-disabled");
        $("#commentsDiv-postButton a").attr("onclick", "return false");
    }
}

tfc.Social.OnLoveReaction = function (params) {
    if (params.love == 'True')
        $(".gig-reaction-button-text").html('Unlove');
    else
        $(".gig-reaction-button-text").html('Love');
}

tfc.Social.ShowFriendSelectorUI = function (params) {
    var obj = {};

    obj.onSelectionDone = function (eventObj) {
        var friends = eventObj.friends;
        if (friends != null) {
            var friendsArr = friends['arr'];
            if (friendsArr != null && friendsArr.length > 0) {
                for (var i = 0; i < friendsArr.length; i++) {
                    var friend = friendsArr[i];
                    var name = 'Friend\'s Name is :' + friend.identities['facebook'].providerUID + '(' + friend['nickname'] + ') (' + friend['UID'] + ')';
                    if (friend.identities['facebook'] != null) {
                        var target = 'facebook:' + friend.identities['facebook'].providerUID;
                        obj.postToWall(target);
                    }
                    else {
                        var target = friend['UID'];
                        obj.postToWall(target);
                    }
                    //console.log(name);
                }
            }
        }
    }

    obj.postToWall = function (target) {
        var act = new gigya.socialize.UserAction();
        act.setUserMessage('uMessage');
        act.setTitle("Title");
        act.setLinkBack("http://tfc.tv");
        act.setDescription("This is my Description");
        act.addActionLink("Read More", "http://tfc.tv");

        var params = { userAction: act, target: target, shortURLs: 'never', cid: 'RAK' };
        gigya.socialize.publishUserAction(params);
    }

    gigya.socialize.showFriendSelectorUI({
        containerID: params.container,
        width: params.width,
        height: params.height,
        enabledProviders: params.enabledProviders,
        onSelectionDone: obj.onSelectionDone
    });

    return obj;
}