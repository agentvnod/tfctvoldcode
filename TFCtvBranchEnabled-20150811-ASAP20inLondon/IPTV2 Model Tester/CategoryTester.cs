using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class CategoryTester
    {
        public static void CategoryTest()
        {
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(2);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == 46);

            var regionalNews = (Category)context.CategoryClasses.Find(767);
            var showIds = service.GetAllOnlineShowIds("US", regionalNews);
            int[] setofShows = showIds.ToArray();
            var registDt = DateTime.Now;
            var list = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StartDate <= registDt && c.EndDate >= registDt && c.StatusId == 1).OrderBy(c => c.CategoryName).ThenBy(c => c.StartDate).ToList();
        }

        public static void FreeVideoTester()
        {

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(2);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == 46);

            int episodeId = 23397;
            var episode = context.Episodes.Find(episodeId);
            EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == 1);

            var packageIds = category.Show.GetPackageProductIds(offering, "US", RightsType.Online);


        }
    }
}
