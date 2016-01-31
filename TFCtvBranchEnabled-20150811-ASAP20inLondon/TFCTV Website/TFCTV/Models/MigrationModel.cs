using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCTV.Models
{
    public class LicenseDisplay
    {
        public int PackageId { get; set; }

        public string PackageName { get; set; }

        public string PackageDescription { get; set; }

        public DateTime LicenseStartDate { get; set; }

        public string LicenseStartDateStr { get; set; }

        public DateTime LicenseEndDate { get; set; }

        public string LicenseEndDateStr { get; set; }

        public string LicensePurchaseId { get; set; }
    }
}