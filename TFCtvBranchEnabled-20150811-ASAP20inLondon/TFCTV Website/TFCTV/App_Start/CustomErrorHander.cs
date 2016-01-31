using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using WorldDomination.Web.Mvc;
using TFCTV.App_Start;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(CustomErrorHander), "PreStart")]
namespace TFCTV.App_Start
{
    public static class CustomErrorHander
    {
        public static void PreStart()
        {
            // Register the custom error handling module.
            DynamicModuleUtility.RegisterModule(typeof (CustomErrorHandlingModule));
        }
    }
}