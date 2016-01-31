using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using TFCTV.Helpers;
using EngagementsModel;
using Gigya.Socialize.SDK;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TFCtv_Background_Cache__Updater
{
    class EngagementCacheRefresher
    {


        public void FillMostLovedCelebrityCache(IPTV2Entities context, EngagementsEntities socialContext, int offeringId, int serviceId, TimeSpan cacheDuration, int SOCIAL_LOVE)
        {
            List<MostLovedCelebritiesDisplay> mostLovedCelebrities = null;
            try
            {
                var countries = context.Countries.ToList();
                foreach (var country in countries)
                {
                    try
                    {
                        string cacheKey = "SEGMLC1:O;C:" + country.Code;
                        mostLovedCelebrities = new List<MostLovedCelebritiesDisplay>();

                        //var mostLovedCelebrityReactions = socialContext.CelebrityReactions
                        //    .Where(rtId => rtId.ReactionTypeId == SOCIAL_LOVE)
                        //    .GroupBy(celeb => celeb.CelebrityId)
                        //    .Select(celeb => new
                        //    {
                        //        celebrityId = celeb.Key,
                        //        totalLoved = celeb.Count()
                        //    }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(5);
                        var mostLovedCelebrityReactions = socialContext.CelebrityReactionSummaries
                            .Where(rtId => rtId.ReactionTypeId == SOCIAL_LOVE)
                            .Select(celeb => new
                            {
                                celebrityId = celeb.CelebrityId,
                                totalLoved = celeb.Total7Days
                            }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(20);

                        try
                        {
                            int count = 0;
                            foreach (var item in mostLovedCelebrityReactions)
                            {

                                var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == item.celebrityId && c.StatusId == 1);
                                if (celebrity != null)
                                {
                                    try
                                    {
                                        mostLovedCelebrities.Add(new MostLovedCelebritiesDisplay()
                                        {
                                            celebrityId = item.celebrityId,
                                            celebrityName = celebrity.FullName,
                                            totalLove = item.totalLoved
                                        });
                                        count++;
                                    }
                                    catch (Exception e) { Trace.Fail(e.Message); }

                                }
                                if (count > 4)
                                    break;
                            }
                            DataCache.Cache.Put(cacheKey, mostLovedCelebrities, cacheDuration);
                        }
                        catch (Exception e) { Trace.Fail(e.Message); }
                    }
                    catch (Exception e) { Trace.Fail(e.Message); }
                }
            }
            catch (Exception) { }
        }

        public void FillMostLovedEpisodeCache(IPTV2Entities context, EngagementsEntities socialContext, int offeringId, int serviceId, TimeSpan cacheDuration, int SOCIAL_LOVE, int FreeTvCategoryId)
        {
            List<MostLovedEpisodesDisplay> mostLovedEpisodes = null;
            try
            {
                var countries = context.Countries.ToList();
                foreach (var c in countries)
                {
                    try
                    {
                        string cacheKey = "SEGMLE1:O;C:" + c.Code;
                        mostLovedEpisodes = new List<MostLovedEpisodesDisplay>();

                        //var mostLovedEpisodeReactions = socialContext.EpisodeReactions
                        //    .Where(rtId => rtId.ReactionTypeId == SOCIAL_LOVE)
                        //    .GroupBy(e => e.EpisodeId)
                        //    .Select(e => new
                        //    {
                        //        EpisodeId = e.Key,
                        //        TotalLoved = e.Count()
                        //    }).ToList().OrderByDescending(e => e.TotalLoved).Take(5);
                        var mostLovedEpisodeReactions = socialContext.EpisodeReactionSummaries
                            .Where(rtId => rtId.ReactionTypeId == SOCIAL_LOVE)
                            .Select(e => new
                            {
                                EpisodeId = e.EpisodeId,
                                TotalLoved = e.Total7Days
                            }).ToList().OrderByDescending(e => e.TotalLoved).Take(20);

                        try
                        {
                            int count = 0;
                            foreach (var item in mostLovedEpisodeReactions)
                            {
                                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == item.EpisodeId && e.OnlineStatusId == 1);
                                if (episode != null)
                                {
                                    var category = episode.EpisodeCategories.FirstOrDefault(ec => ec.CategoryId != FreeTvCategoryId);
                                    if (category != null)
                                    {
                                        try
                                        {
                                            mostLovedEpisodes.Add(new MostLovedEpisodesDisplay()
                                            {
                                                showId = category.CategoryId,
                                                showName = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? category.Show.Description : episode.EpisodeName,
                                                dateAired = episode.DateAired.Value.ToString("MMMM dd, yyyy"),
                                                episodeId = episode.EpisodeId,
                                                episodeName = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? episode.DateAired.Value.ToString("MMM dd, yyyy") : episode.EpisodeName,
                                                totalLove = item.TotalLoved,
                                                slug = GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", category.Show.Description, episode.DateAired.Value.ToString("MMMM d yyyy"))),
                                                showSlug = GetSlug(category.Show.Description)
                                            });
                                            count++;
                                        }
                                        catch (Exception e) { Trace.Fail(e.Message); }
                                    }
                                }
                                if (count > 4)
                                    break;
                            }
                            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedEpisodes);
                            DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                        }
                        catch (Exception e) { Trace.Fail(e.Message); }
                    }
                    catch (Exception e) { Trace.Fail(e.Message); }
                }
            }
            catch (Exception e) { Trace.Fail(e.Message); }
        }

        public void FillMostLovedShowCache(IPTV2Entities context, EngagementsEntities socialContext, int offeringId, int serviceId, TimeSpan cacheDuration, int SOCIAL_LOVE, int Entertainment)
        {
            List<MostLovedShowsDisplay> mostLovedShows = null;
            try
            {
                var countries = context.Countries.ToList();

                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == offeringId);
                var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);

                foreach (var c in countries)
                {
                    try
                    {
                        string cacheKey = "SEGMLS1:O;C:" + c.Code;
                        mostLovedShows = new List<MostLovedShowsDisplay>();
                        Category category = (Category)context.CategoryClasses.FirstOrDefault(cc => cc.CategoryId == Entertainment && cc.StatusId == 1);
                        if (category != null)
                        {
                            SortedSet<int> showIds = service.GetAllOnlineShowIds(c.Code, category);

                            //var showReactions = socialContext.ShowReactions
                            //.Where(cc => cc.ReactionTypeId == SOCIAL_LOVE && showIds.Contains(cc.CategoryId))
                            //.GroupBy(cc => cc.CategoryId)
                            //.Select(cc => new
                            //{
                            //    categoryId = cc.Key,
                            //    totalLove = cc.Count()
                            //}).ToList().OrderByDescending(cc => cc.totalLove).Take(5);
                            var showReactions = socialContext.ShowReactionSummaries
                                .Where(cc => cc.ReactionTypeId == SOCIAL_LOVE && showIds.Contains(cc.CategoryId))
                                .Select(cc => new
                                {
                                    categoryId = cc.CategoryId,
                                    totalLove = cc.Total7Days
                                }).ToList().OrderByDescending(cc => cc.totalLove).Take(5);

                            try
                            {
                                foreach (var item in showReactions)
                                {
                                    var categoryClass = context.CategoryClasses.FirstOrDefault(cc => cc.CategoryId == item.categoryId);
                                    if (categoryClass != null)
                                    {
                                        try
                                        {
                                            mostLovedShows.Add(new MostLovedShowsDisplay()
                                            {
                                                categoryId = item.categoryId,
                                                totalLove = item.totalLove,
                                                categoryName = categoryClass.Description,
                                                slug = GetSlug(categoryClass.Description)
                                            });
                                        }
                                        catch (Exception e) { Trace.Fail(e.Message); }
                                    }
                                }
                                var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedShows);
                                DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                            }
                            catch (Exception e) { Trace.Fail(e.Message); }
                        }
                    }
                    catch (Exception e) { Trace.Fail(e.Message); }
                }
            }
            catch (Exception e) { Trace.Fail(e.Message); }
        }

        public void FillTopReviewersCache(IPTV2Entities context, EngagementsEntities socialContext, int offeringId, int serviceId, TimeSpan cacheDuration, int SOCIAL_RATING, string AssetsBaseUrl)
        {
            List<TopReviewersDisplay> topReviewers = null;
            try
            {
                var countries = context.Countries.ToList();
                foreach (var c in countries)
                {
                    try
                    {
                        string cacheKey = "SEGTR1:O;C:" + c.Code;
                        topReviewers = new List<TopReviewersDisplay>();

                        var topReviewersAll = socialContext.EpisodeReactions
                            .Where(rtId => rtId.ReactionTypeId == SOCIAL_RATING)
                            .Select(user => new { user.UserId, user.Reactionid })
                            .Union(socialContext.ShowReactions.Where(rtId => rtId.ReactionTypeId == SOCIAL_RATING)
                            .Select(user => new { user.UserId, user.Reactionid })).GroupBy(user => user.UserId)
                            .Select(user => new
                            {
                                userId = user.Key,
                                totalReview = user.Count()
                            }).ToList().OrderByDescending(user => user.totalReview).Take(5);

                        try
                        {
                            foreach (var item in topReviewersAll)
                            {
                                var user = context.Users.FirstOrDefault(u => u.UserId == item.userId);
                                if (user != null)
                                {
                                    try
                                    {
                                        topReviewers.Add(new TopReviewersDisplay()
                                        {
                                            userId = item.userId.ToString(),
                                            userName = String.Format("{0} {1}", user.FirstName, user.LastName),
                                            userPhoto = GetUserImage(item.userId, AssetsBaseUrl),
                                            totalReview = item.totalReview
                                        });
                                    }
                                    catch (Exception e) { Trace.Fail(e.Message); }
                                }
                            }
                            DataCache.Cache.Put(cacheKey, topReviewers, cacheDuration);
                        }
                        catch (Exception e) { Trace.Fail(e.Message); }
                    }
                    catch (Exception e) { Trace.Fail(e.Message); }
                }
            }
            catch (Exception e) { Trace.Fail(e.Message); }
        }

        private string GetUserImage(Guid uid, string AssetsBaseUrl)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            collection.Add("uid", uid.ToString());
            var imageUrl = "";
            try
            {
                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));
                imageUrl = res.GetData().GetString("thumbnailURL");
            }
            catch (Exception)
            {
                imageUrl = String.Format("{0}/{1}", AssetsBaseUrl, "content/images/social/profilephoto.png");
            }
            return imageUrl;
            //return res.GetData().ToJsonString();
        }

        public void FillMostLovedCelebrityCache1(IPTV2Entities context, EngagementsEntities socialContext, int offeringId, int serviceId, TimeSpan cacheDuration, int SOCIAL_LOVE)
        {
            List<MostLovedCelebritiesDisplay> mostLovedCelebrities = null;
            try
            {
                try
                {
                    string cacheKey = "SEGMLC1:O;C:";
                    mostLovedCelebrities = new List<MostLovedCelebritiesDisplay>();
                    var mostLovedCelebrityReactions = socialContext.CelebrityReactionSummaries
                        .Where(rtId => rtId.ReactionTypeId == SOCIAL_LOVE)
                        .Select(celeb => new
                        {
                            celebrityId = celeb.CelebrityId,
                            totalLoved = celeb.Total7Days
                        }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(20);

                    try
                    {
                        int count = 0;
                        foreach (var item in mostLovedCelebrityReactions)
                        {

                            var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == item.celebrityId && c.StatusId == 1);
                            if (celebrity != null)
                            {
                                try
                                {
                                    mostLovedCelebrities.Add(new MostLovedCelebritiesDisplay()
                                    {
                                        celebrityId = item.celebrityId,
                                        celebrityName = celebrity.FullName,
                                        totalLove = item.totalLoved,
                                        slug = GetSlug(celebrity.FullName)
                                    });
                                    count++;
                                }
                                catch (Exception e) { Trace.Fail(e.Message); }

                            }
                            if (count > 4)
                                break;
                        }
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedCelebrities);
                        DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception e) { Trace.Fail(e.Message); }
                }
                catch (Exception e) { Trace.Fail(e.Message); }
            }
            catch (Exception) { }
        }

        public void FillTopReviewersCache1(IPTV2Entities context, EngagementsEntities socialContext, int offeringId, int serviceId, TimeSpan cacheDuration, int SOCIAL_RATING, string AssetsBaseUrl)
        {
            List<TopReviewersDisplay> topReviewers = null;
            try
            {
                try
                {
                    string cacheKey = "SEGTR1:O;C:";
                    topReviewers = new List<TopReviewersDisplay>();

                    var topReviewersAll = socialContext.EpisodeReactions
                        .Where(rtId => rtId.ReactionTypeId == SOCIAL_RATING)
                        .Select(user => new { user.UserId, user.Reactionid })
                        .Union(socialContext.ShowReactions.Where(rtId => rtId.ReactionTypeId == SOCIAL_RATING)
                        .Select(user => new { user.UserId, user.Reactionid })).GroupBy(user => user.UserId)
                        .Select(user => new
                        {
                            userId = user.Key,
                            totalReview = user.Count()
                        }).ToList().OrderByDescending(user => user.totalReview).Take(5);

                    try
                    {
                        foreach (var item in topReviewersAll)
                        {
                            var user = context.Users.FirstOrDefault(u => u.UserId == item.userId);
                            if (user != null)
                            {
                                try
                                {
                                    topReviewers.Add(new TopReviewersDisplay()
                                    {
                                        userId = item.userId.ToString(),
                                        userName = String.Format("{0} {1}", user.FirstName, user.LastName),
                                        userPhoto = GetUserImage(item.userId, AssetsBaseUrl),
                                        totalReview = item.totalReview
                                    });
                                }
                                catch (Exception e) { Trace.Fail(e.Message); }
                            }
                        }
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(topReviewers);
                        DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception e) { Trace.Fail(e.Message); }
                }
                catch (Exception e) { Trace.Fail(e.Message); }

            }
            catch (Exception e) { Trace.Fail(e.Message); }
        }

        public string GetSlug(string slug)
        {
            try
            {
                Regex rx = new Regex(@"\/+");
                slug = rx.Replace(slug, " ");
                rx = new Regex(@"[^a-zA-Z0-9\-\s]");
                slug = rx.Replace(slug, "");
                rx = new Regex(@"\s+");
                slug = rx.Replace(slug, " ");
                rx = new Regex(@"\s");
                slug = rx.Replace(slug, "-");
                return slug.ToLower();
            }
            catch (Exception) { }
            return String.Empty;

        }
    }
}
