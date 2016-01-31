using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IPTV2_Model;

namespace TFCTV.Models
{
    public class HelpCenterCategoryModel
    {
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }
    }

    public class HelpCenterQuestionModel
    {
        public int id { get; set; }

        public string Q { get; set; }

        public string Answer { get; set; }

        public int? SubCategoryId { get; set; }
    }

    public class ErrorResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public string CCEnrollmentStatusMessage { get; set; }
        public Transaction transaction { get; set; }
        public Product product { get; set; }
        public ProductPrice price { get; set; }
        public string ProductType { get; set; }
    }
}