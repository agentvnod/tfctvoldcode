using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Category
    {
        public SortedSet<int> GetAllParentCategories(CategoryClass category, TimeSpan? CacheDuration = null)
        {
            SortedSet<int> parentCategoryList = null;

            var cache = DataCache.Cache;
            string cacheKey = "GAParentCat:Cat:" + category.CategoryId;
            parentCategoryList = (SortedSet<int>)cache[cacheKey];
            if (parentCategoryList == null)
            {
                parentCategoryList = new SortedSet<int>();
                var parentCategories = category.CategoryClassParentCategories;
                var categoryParentIds = parentCategories.Select(c => c.ParentId);
                parentCategoryList.UnionWith(categoryParentIds);
                foreach (var item in parentCategories)
                {
                    var categoryIds = GetAllParentCategories(item.ParentCategory, CacheDuration);
                    if (categoryIds != null)
                        parentCategoryList.UnionWith(categoryIds);
                }
                cache.Put(cacheKey, parentCategoryList, CacheDuration != null ? (TimeSpan)CacheDuration : DataCache.CacheDuration);
            }
            return (parentCategoryList);
        }

        public SortedSet<int> GetAllParentCategories(TimeSpan? CacheDuration = null)
        {
            SortedSet<int> parentCategoryList = null;
            var cache = DataCache.Cache;
            string cacheKey = "GAPRNTCAT:Cat:" + CategoryId;
            parentCategoryList = (SortedSet<int>)cache[cacheKey];
            if (parentCategoryList == null)
            {
                parentCategoryList = new SortedSet<int>();
                var parentCategories = CategoryClassParentCategories;
                var categoryParentIds = parentCategories.Select(c => c.ParentId);
                parentCategoryList.UnionWith(categoryParentIds);
                foreach (var item in parentCategories)
                {
                    var categoryIds = GetAllParentCategories(item.ParentCategory, CacheDuration);
                    if (categoryIds != null)
                        parentCategoryList.UnionWith(categoryIds);
                }
                cache.Put(cacheKey, parentCategoryList, CacheDuration != null ? (TimeSpan)CacheDuration : DataCache.CacheDuration);
            }
            return (parentCategoryList);
        }
    }
}
