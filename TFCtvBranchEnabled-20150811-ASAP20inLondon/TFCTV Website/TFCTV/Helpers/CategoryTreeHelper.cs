using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using IPTV2_Model;

namespace TFCTV.Helpers
{
    public static class CategoryTreeHelper
    {
        public static List<ShowLookUpObject> GetShowsOnCurrentOffering()
        {
            var context = new IPTV2Entities();
            var service = context.Offerings.Find(GlobalConfig.offeringId).Services.Where(p => p.PackageId == GlobalConfig.serviceId && p.StatusId == GlobalConfig.Visible).Single();

            List<ShowLookUpObject> slo = new List<ShowLookUpObject>();
            foreach (PackageCategory pkg_category in service.Categories)
            {
                ShowLookUpObject lookupObj = new ShowLookUpObject();
                CategoryClass categoryClass = context.CategoryClasses.Find(pkg_category.CategoryId);
                lookupObj.MainCategoryId = pkg_category.CategoryId;
                lookupObj.MainCategory = categoryClass.CategoryName;
                Traverse(categoryClass, ref lookupObj, ref slo);
            }

            return slo;

            //HttpContext.Current.Cache.Insert("ShowList", slo, null, DateTime.Now.AddHours(1), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        private static void Traverse(CategoryClass category, ref ShowLookUpObject lookupObj, ref List<ShowLookUpObject> slo)
        {
            if (category is Show)
            {
                if (category.CategoryId == lookupObj.SubCategoryId)
                {
                    lookupObj.SubCategoryId = lookupObj.MainCategoryId;
                    lookupObj.SubCategory = lookupObj.MainCategory;
                }
                lookupObj.ShowId = category.CategoryId;
                lookupObj.Show = category.CategoryName;
                slo.Add(lookupObj);
            }
            else
            {
                var subcategories = category.CategoryClassSubCategories.Select(s => s.SubCategory).Where(s => s.StatusId == 1);

                foreach (CategoryClass c in subcategories)
                {
                    ShowLookUpObject lookupObj_t = new ShowLookUpObject();
                    lookupObj_t.MainCategoryId = lookupObj.MainCategoryId;
                    lookupObj_t.MainCategory = lookupObj.MainCategory;
                    var sub_category = category.CategoryClassSubCategories.Where(sc => sc.CategoryId == c.CategoryId).Single();
                    lookupObj_t.SubCategoryId = (lookupObj.SubCategoryId == null) ? sub_category.SubCategory.CategoryId : lookupObj.SubCategoryId;
                    lookupObj_t.SubCategory = (lookupObj.SubCategory == null) ? sub_category.SubCategory.CategoryName : lookupObj.SubCategory;
                    Traverse(c, ref lookupObj_t, ref slo);
                }
            }
        }
    }
}