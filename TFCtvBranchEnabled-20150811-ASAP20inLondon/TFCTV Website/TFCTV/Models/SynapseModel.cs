using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCTV.Models
{
    public class SynapseModel
    {
    }

    public class SynapseEpisode
    {
        public int id { get; set; }

        public string name { get; set; }

        public string dateaired { get; set; }

        public string synopsis { get; set; }

        public string show { get; set; }

        public string image { get; set; }

        public string episodelength { get; set; }

        public int? episodenumber { get; set; }

        public string expirydate { get; set; }
        public string oexpirydate { get; set; }
        public string mexpirydate { get; set; }
        public int statusid { get; set; }
    }

    public class SynapseShow
    {
        public int id { get; set; }

        public string name { get; set; }

        public string image { get; set; }

        public string banner { get; set; }

        public string blurb { get; set; }

        public string type { get; set; }

        public int parentId { get; set; }
        public string parent { get; set; }

        public string dateairedstr { get; set; }

        public virtual List<SynapseEpisode> episodes { get; set; }
    }

    public class SynapseCategory
    {
        public int id { get; set; }

        public string name { get; set; }

        public SynapseShow feature { get; set; }

        public virtual List<SynapseShow> shows { get; set; }
    }

    public class SynapseCelebrity
    {
        public int id { get; set; }

        public string name { get; set; }

        public string image { get; set; }

        public string birthday { get; set; }

        public string birthplace { get; set; }

        public string height { get; set; }

        public string weight { get; set; }

        public string description { get; set; }
    }

    public class SynapseHomepage
    {
        public Helpers.JsonCarouselItem carousel { get; set; }

        public List<SynapseShow> show { get; set; }

        public List<SynapseCelebrity> celebrity { get; set; }
    }

    public class SynapseCookie
    {
        public string cookieName { get; set; }

        public string cookieValue { get; set; }

        public string cookieDomain { get; set; }

        public string cookiePath { get; set; }

        public string UID { get; set; }

        public string UIDSignature { get; set; }

        public int signatureTimestamp { get; set; }
    }

    public class SynapseToken
    {
        public string uid { get; set; }

        public string token { get; set; }

        public int expire { get; set; }
    }

    public class SynapseResponse
    {
        public int errorCode { get; set; }

        public string errorMessage { get; set; }

        public string errorDetails { get; set; }

        public string callId { get; set; }

        public object data { get; set; }

        public object info { get; set; }
    }

    public class SynapseUserInfo
    {

        public Guid uid { get; set; }
        public string firstName { get; set; }

        public string lastName { get; set; }

        public string email { get; set; }

        public string City { get; set; }
        public string CountryCode { get; set; }
        public string State { get; set; }
    }

    public class SynapseSearchResult
    {
        public int counter { get; set; }

        public string url { get; set; }

        virtual public SynapseUrlBreakdown controller { get; set; }

        public string title { get; set; }

        public string text { get; set; }

        public string image { get; set; }

        public int? score { get; set; }

        public string lastModified { get; set; }
    }

    public class SynapseUrlBreakdown
    {
        public string controller { get; set; }

        public string action { get; set; }

        public string id { get; set; }
    }

    public class SynapseShow1
    {
        public int id { get; set; }

        public string name { get; set; }

        public string image { get; set; }

        public string banner { get; set; }

        public string blurb { get; set; }

        public string type { get; set; }

        public int parentId { get; set; }
        public string parent { get; set; }

        public string dateairedstr { get; set; }
        
    }

    public class SynapseCategory1
    {
        public int id { get; set; }

        public string name { get; set; }

        public SynapseShow1 feature { get; set; }

        public virtual List<SynapseShow1> shows { get; set; }
    }
}