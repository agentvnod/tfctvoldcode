/* File Created: January 26, 2012 */

$(document).ready(function () {
    //    var APIKey = '';
    //    var socializeUrl = 'https://cdns.gigya.com/JS/socialize.js?apikey=' + APIKey;
    //    var socialPluginUrl = '/Scripts/tfc.social.plugin.js';

    //    $.getScript(socializeUrl, function () {
    //        $.getScript(socialPluginUrl, function () {
    //            loaded = true;
    config = { apikey: APIKey, connectWithoutLoginBehavior: 'alwaysLogin' }
    var params = { container: 'socialConnections', width: 120, height: 20, style: 'standard', enabledProviders: 'facebook,twitter,yahoo,google,messenger,foursquare,linkedin,myspace' };
    tfc.Social.Login(params);
    //        });
    //    });
});    