/* File Created: September 12, 2012 */
tfc.Socialize = function () {
    var obj = {};
    return obj;
}

// Sharing V2
tfc.Socialize.Share = function (params) {
    var obj = {};

    obj.onSendDone = function (event) {
        if (params.isLoggedIn) {
            var reactionTypeId = _SHARE;
            jQuery.post(params.postUrl,
                {
                    userId: params.userAction.actorUID,
                    reactionTypeId: reactionTypeId,
                    idx: params.idx,
                    type: params.type,
                    action: 'add'
                }, function (data) {
                    $.post("/Ajax/NotifyAction", params.nattributes);
                });
        }
    }

    obj.onLoad = function () {
        if (!params.isLoggedIn) { $('#' + params.container).append($('.dcover').html()); }
    }

    obj.Init = function () {
        gigya.socialize.showShareBarUI(
        {
            containerID: params.container,
            shareButtons: params.shareButtons,
            showCounts: params.showCounts,
            grayedOutScreenOpacity: 20,
            userAction: tfc.Socialize.CreateUserAction(params.userAction),
            shortURLs: 'whenRequired',
            operationMode: 'simpleShare',
            onLoad: obj.onLoad,
            onSendDone: obj.onSendDone
        }
    );
    }
    obj.Init();
    return obj;
}

// Love V2
tfc.Socialize.Love = function (params) {
    var obj = {};


    obj.CreateLoveUserAction = function () {
        var action = tfc.Socialize.CreateUserAction(params.LoveUserActionParams);
        var internalParams = { UID: action.actorUID, userAction: action, cid: params.LoveUserActionParams.title, scope: 'internal', privacy: params.LoveUserActionParams.privacy, feedID: params.LoveUserActionParams.feedID };
        if (params.LoveUserActionParams.internal && params.isLoggedIn)
            tfc.Socialize.PublishUserAction(internalParams);
    }

    obj.updateReaction = function (event, isReact) {
        if (params.isLoggedIn) {
            var reactionTypeId = tfc.Socialize.GetEngagementValue(event.reaction.ID);
            jQuery.post(params.postUrl,
                {
                    userId: params.userAction.actorUID,
                    reactionTypeId: reactionTypeId,
                    idx: params.idx,
                    type: params.type,
                    action: (isReact) ? 'add' : 'remove'
                }, function (data) {
                    console.log('Love Complete');
                });
        }
    }

    obj.onReactionClicked = function (event) {
        var t = 'Unlove';
        event.reaction.text = t;
        $('.gig-reaction-button-text').html(t);
        obj.updateReaction(event, true);
        obj.CreateLoveUserAction();
    }

    obj.onReactionUnclicked = function (event) {
        var t = 'Love';
        event.reaction.text = t;
        $('.gig-reaction-button-text').html(t);
        obj.updateReaction(event, false);
    }

    obj.onSendDone = function (event) {
        // POST
    }

    obj.onLoad = function () {
        if (!params.isLoggedIn) { $('#' + params.container).append($('.dcoverL').html()); }
    }

    obj.Init = function () {
        gigya.services.socialize.showReactionsBarUI(
            {
                barID: params.barID,
                showCounts: 'right',
                containerID: params.container,
                reactions: params.myReactions,
                userAction: tfc.Socialize.CreateUserAction(params.userAction),
                showSuccessMessage: false,
                noButtonBorders: false,
                cid: params.cid,
                shortURLs: 'never',
                onReactionClicked: obj.onReactionClicked,
                onReactionUnclicked: obj.onReactionUnclicked,
                onSendDone: obj.onSendDone,
                onLoad: obj.onLoad
            }
        );
    }
    obj.Init();
    return obj;
}

// Post Comment
tfc.Socialize.PostComment = function (params) {
    var obj = {};
    var responseObj;
    obj.postProcess = function (response) {
        //console.log(response);
        responseObj = response;
    }

    obj.Init = function () {
        gigya.comments.postComment(
            {
                categoryID: params.categoryID,
                streamID: params.streamID,
                userAction: params.userAction,
                commentText: params.commentText,
                cid: params.cid,
                callback: obj.postProcess
            }
        );
    }
    obj.Init();

    return responseObj;
}

tfc.Socialize.CreateUserAction = function (params) {
    var action = new gigya.socialize.UserAction();
    action.setTitle(params.title);
    action.setLinkBack(params.url);
    action.setDescription(params.description);
    action.setSubtitle(params.subtitle);
    if (params.usermessage != undefined)
        action.setUserMessage(params.usermessage);
    action.addMediaItem({ type: 'image', src: params.src, href: params.url });
    if (params.actorUID != undefined)
        action.actorUID = params.actorUID;
    return action;
}

tfc.Socialize.PublishUserAction = function (params) {
    var action = {
        UID: params.UID,
        userAction: params.userAction,
        callback: function (response) { console.log(response); },
        enabledProviders: null,
        cid: params.cid,
        enabledProviders: null,
        scope: params.scope,
        privacy: params.privacy,
        feedID: params.feedID
    }
    if (params.scope == "external") {
        action.enabledProviders = params.provider;
        action.feedID = null;
        action.privacy = null;
    }
    gigya.socialize.publishUserAction(action);
}

// Love No Count
tfc.Socialize.LoveNoCount = function (params) {
    var obj = {};

    obj.UpdateReactionCount = function () {
        gigya.services.socialize.getReactionsCount(
            {
                barID: params.barID,
                buttonIDs: params.buttonId,
                callback: function (response) {
                    var myCounts = response['counts']; $('#' + params.lcontainer).html(myCounts[0].count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ","));
                    if (myCounts[0].count > 0)
                        $('#' + params.lcontainer).show();
                    else
                        $('#' + params.lcontainer).hide();
                }
            }
        );
    }
    obj.CreateLoveUserAction = function () {
        var action = tfc.Socialize.CreateUserAction(params.LoveUserActionParams);
        var internalParams = { UID: action.actorUID, userAction: action, cid: params.LoveUserActionParams.title, scope: 'internal', privacy: params.LoveUserActionParams.privacy, feedID: params.LoveUserActionParams.feedID };
        if (params.LoveUserActionParams.internal && params.isLoggedIn)
            tfc.Socialize.PublishUserAction(internalParams);
    }

    obj.updateReaction = function (event, isReact) {
        if (params.isLoggedIn) {
            var reactionTypeId = tfc.Socialize.GetEngagementValue(event.reaction.ID);
            jQuery.post(params.postUrl,
                {
                    userId: params.userAction.actorUID,
                    reactionTypeId: reactionTypeId,
                    idx: params.idx,
                    type: params.type,
                    action: (isReact) ? 'add' : 'remove'
                }, function (data) {
                    $.post("/Ajax/NotifyAction", params.nattributes);
                }).done(function () { obj.UpdateReactionCount(); });
        }
    }

    obj.onReactionClicked = function (event) {
        var t = 'Unlove';
        event.reaction.text = t;
        $('.gig-reaction-button-text').html(t);
        obj.updateReaction(event, true);
        obj.CreateLoveUserAction();
    }

    obj.onReactionUnclicked = function (event) {
        var t = 'Love';
        event.reaction.text = t;
        $('.gig-reaction-button-text').html(t);
        obj.updateReaction(event, false);
    }

    obj.onSendDone = function (event) {
        // POST
    }

    obj.onLoad = function () {
        if (!params.isLoggedIn) { $('#' + params.container).append($('.dcoverL').html()); }
    }

    obj.Init = function () {
        gigya.services.socialize.showReactionsBarUI(
            {
                barID: params.barID,
                showCounts: 'none',
                containerID: params.container,
                reactions: params.myReactions,
                userAction: tfc.Socialize.CreateUserAction(params.userAction),
                showSuccessMessage: false,
                noButtonBorders: false,
                cid: params.cid,
                shortURLs: 'never',
                onReactionClicked: obj.onReactionClicked,
                onReactionUnclicked: obj.onReactionUnclicked,
                onSendDone: obj.onSendDone,
                onLoad: obj.onLoad
            }
        );
    }
    obj.Init();
    return obj;
}

// Get Reaction Count
tfc.Socialize.GetReactionCount = function (params) {
    var obj = {};

    obj.onRequestDone = function (response) {
        var myCounts = response['counts'];
        $('#' + params.lcontainer).html(myCounts[0].count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ","));
        if (myCounts[0].count > 0)
            $('#' + params.lcontainer).show();

    }

    obj.Init = function () {
        gigya.services.socialize.getReactionsCount(
            {
                barID: params.barID,
                buttonIDs: params.buttonId,
                callback: obj.onRequestDone
            }
        );
    }
    obj.Init();
    return obj;
}

tfc.Socialize.LoveNoCountWithTemplate = function (params) {
    var obj = {};

    obj.UpdateReactionCount = function () {
        gigya.services.socialize.getReactionsCount(
            {
                barID: params.barID,
                buttonIDs: params.buttonId,
                callback: function (response) {
                    var myCounts = response['counts']; $('#' + params.lcontainer).html(myCounts[0].count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ","));
                    if (myCounts[0].count > 0)
                        $('#' + params.lcontainer).show();
                    else
                        $('#' + params.lcontainer).hide();
                }
            }
        );
    }
    obj.CreateLoveUserAction = function () {
        var action = tfc.Socialize.CreateUserAction(params.LoveUserActionParams);
        var internalParams = { UID: action.actorUID, userAction: action, cid: params.LoveUserActionParams.title, scope: 'internal', privacy: params.LoveUserActionParams.privacy, feedID: params.LoveUserActionParams.feedID };
        if (params.LoveUserActionParams.internal && params.isLoggedIn)
            tfc.Socialize.PublishUserAction(internalParams);
    }

    obj.updateReaction = function (event, isReact) {
        if (params.isLoggedIn) {
            var reactionTypeId = tfc.Socialize.GetEngagementValue(event.reaction.ID);
            jQuery.post(params.postUrl,
                {
                    userId: params.userAction.actorUID,
                    reactionTypeId: reactionTypeId,
                    idx: params.idx,
                    type: params.type,
                    action: (isReact) ? 'add' : 'remove'
                }, function (data) {
                    $.post("/Ajax/NotifyAction", params.nattributes);
                }).done(function () { obj.UpdateReactionCount(); });
        }
    }

    obj.onReactionClicked = function (event) {
        var t = 'Unlove';
        event.reaction.text = t;
        $('.gig-reaction-button-text').html(t);
        $('#LoveImg').attr('src', params.myReactions[0].iconImgDown);
        obj.updateReaction(event, true);
        obj.CreateLoveUserAction();
    }

    obj.onReactionUnclicked = function (event) {
        var t = 'Love';
        event.reaction.text = t;
        $('.gig-reaction-button-text').html(t);
        $('#LoveImg').attr('src', params.myReactions[0].iconImgUp);
        obj.updateReaction(event, false);
    }

    obj.onSendDone = function (event) {
        // POST
    }

    obj.onLoad = function () {
        if (!params.isLoggedIn) { $('#' + params.container).append($('.dcoverL').html()); }
    }

    obj.Init = function () {
        gigya.services.socialize.showReactionsBarUI(
            {
                barID: params.barID,
                showCounts: 'none',
                containerID: params.container,
                reactions: params.myReactions,
                userAction: tfc.Socialize.CreateUserAction(params.userAction),
                showSuccessMessage: false,
                noButtonBorders: false,
                cid: params.cid,
                shortURLs: 'never',
                buttonTemplate: '<div><div class="limage" style="float: left;"><img id="LoveImg" onclick="$onClick" src="$iconImg" /></div><div style="background-color:#d7d7d7;width: 40px; height: 24px; float: left; position: relative; top: 8px; text-align: center;"><div class="lcount1" id="lcount1" style="position: relative; top: 4px;">$count</div></div>',
                onReactionClicked: obj.onReactionClicked,
                onReactionUnclicked: obj.onReactionUnclicked,
                onSendDone: obj.onSendDone,
                onLoad: obj.onLoad
            }
        );
        $('#lcount1').css('font-size', '14px');
        if (params.myReactions[0].text == 'Unlove') {
            $('#LoveImg').attr('src', params.myReactions[0].iconImgDown);
        }
    }
    obj.Init();
    return obj;
}

// Sharing V2
tfc.Socialize.ShareBar = function (params) {
    var obj = {};

    obj.onSendDone = function (event) {
        if (params.isLoggedIn) {
            var reactionTypeId = _SHARE;
            jQuery.post(params.postUrl,
                {
                    userId: params.userAction.actorUID,
                    reactionTypeId: reactionTypeId,
                    idx: params.idx,
                    type: params.type,
                    action: 'add'
                }, function (data) {
                    $.post("/Ajax/NotifyAction", params.nattributes);
                });
        }
    }

    obj.onLoad = function () {
        if (!params.isLoggedIn) { $('#' + params.container).append($('.dcover').html()); }
    }

    obj.Init = function () {
        gigya.socialize.showShareBarUI(
        {
            containerID: params.container,
            shareButtons: params.shareButtons,
            showCounts: params.showCounts,
            grayedOutScreenOpacity: params.grayedOutScreenOpacity,
            userAction: tfc.Socialize.CreateUserAction(params.userAction),
            shortURLs: 'whenRequired',
            iconsOnly: params.iconsOnly,
            operationMode: 'simpleShare',
            onLoad: obj.onLoad,
            onSendDone: obj.onSendDone
        }
    );
    }
    obj.Init();
    return obj;
}

// Reaction types
var _COMMENT = 1;
var _RATINGSANDREVIEW = 2;
var _SHARE = 3;
var _LIKE = 11;
var _LOVE = 12;
tfc.Socialize.GetEngagementValue = function (value) {
    switch (value) {
        case 'like': return _LIKE;
        case 'love': return _LOVE;
        case 'share': return _SHARE;
        case 'rate': return _RATINGSANDREVIEW;
        case 'comment': return _COMMENT;
        default: return 0;
    }
}

