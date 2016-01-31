using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOMS_TFCtv;
using Xunit;

namespace GOMS_TFCtv_Tests
{
    class UserRegistration
    {
        
        
        public static void TestRegisterAllUsers()
        {
            var context = new IPTV2_Model.IPTV2Entities();
            var service = new GomsTfcTv();

            var users = context.Users.Where(u => u.GomsCustomerId == null);
            foreach (var user in users)
            {
                var anotherContext = new IPTV2_Model.IPTV2Entities();
                var resp = service.RegisterUser(anotherContext, user.UserId);
                if (!resp.IsSuccess)
                {
                    Console.WriteLine(user.FirstName + " " + user.LastName + " - " + resp.StatusMessage);
                }
            }
        }

        [Fact]
        public static void TestRegisterUser(System.Guid userId)
        {
            var context = new IPTV2_Model.IPTV2Entities();
            var service = new GomsTfcTv();
            // System.Guid userId = new System.Guid("D9720726-217C-4431-AEAB-468E818132A8");

            var user = context.Users.Find(userId);
            if (user == null)
            {
                var country = context.Countries.Find("US");
                user = new IPTV2_Model.User
                {
                    UserId = userId,
                    EMail = userId.ToString().Substring(10) + "@tfc.tv",
                    FirstName = userId.ToString().Substring(10),
                    LastName = userId.ToString().Substring(10),
                    Password = userId.ToString().Substring(10),
                    RegistrationDate = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    Country = country,
                    State = user.State,
                    City = user.City

                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            var resp = service.RegisterUser(context, userId);
            Assert.Equal(resp.IsSuccess, true);
        }
    }
}
