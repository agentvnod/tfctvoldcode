using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using System.IO;
using System.Collections;


namespace TFCtvContentValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            // ValidateContent()
            ValidateHlsClips();
            Console.ReadLine();
        }

        static void ValidateHlsClips()
        {
            string dirList = @"C:\Users\epaden\Desktop\dirlist.txt";

            int offeringId = 2;
            int serviceId = 46;

            var context = new IPTV2Entities();

            var directoryList = LoadDirectoryList(dirList);

            var offering = context.Offerings.Find(offeringId);

            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);
            var showList = service.GetAllOnlineShowIds("US");
            foreach (var showId in showList)
            {
                var showAsCategory = context.CategoryClasses.Find(showId);

                if (showAsCategory is Show)
                {
                    var show = (Show)showAsCategory;
                    foreach (var ep in show.Episodes.Where(e => e.Episode.OnlineStatusId == 1).OrderBy(ep => ep.Episode.DateAired))
                    {
                        var hlsAssets = from a in ep.Episode.PremiumAssets
                                        from ac in a.Asset.AssetCdns
                                        where ac.CdnId == 2
                                        select ac;

                        foreach (var ha in hlsAssets)
                        {                            
                            var cdnRef = ha.CdnReference;
                            var videoFile = cdnRef.Replace(",500000,800000,1000000,1300000,1500000,.mp4.csmil/master.m3u8", ".mp4");
                            videoFile = videoFile.Substring(videoFile.LastIndexOf('/') + 1).Replace(",", "");
                            if (!directoryList.Contains(videoFile))
                            {
                                Console.WriteLine("Disabling Episode: {0} {1} {2}", show.CategoryName, ep.Episode.Description, ep.Episode.DateAired);
                                ep.Episode.OnlineStatusId = 0;
                                ep.Episode.MobileStatusId = 0;
                                context.SaveChanges();
                            }

                        }

                    }
                }

            }

        }

        static ArrayList LoadDirectoryList(string fileName)
        {
            var list = new ArrayList();

            using (StreamReader inFile = new StreamReader(fileName))
            {
                string inLine = null;
                while (!inFile.EndOfStream)
                {
                    inLine = inFile.ReadLine();
                    if (inLine.Trim().Length > 0)
                    {
                        list.Add(inLine);
                    }
                }
            }

            return (list);
        }

        static void ValidateContent()
        {
            int offeringId = 2;
            int serviceId = 46;

            var context = new IPTV2Entities();


            //var episode = context.Episodes.Find(8341);

            //var assets = from a in episode.PremiumAssets
            //             from ac in a.Asset.AssetCdns
            //             where ac.CdnId == 2
            //             select ac;

            //var c = assets.Count();
            //foreach (var ac in assets)
            //{
            //    int i = 0;
            //}


            var offering = context.Offerings.Find(offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);

            {
                var showList = service.GetAllOnlineShowIds("US");
                var outFilename = @"C:\VS2010 Projects\TFC.tv\TFCtvContentValidator\eplist.txt";
                using (StreamWriter outfile = new StreamWriter(outFilename))
                {
                    outfile.AutoFlush = true;
                    outfile.WriteLine("ShowId\tShowName\tEpisodes\tOnlineStatus\tDate Aired\tEpisode Title\tEpisode #\tVideo Image\tPlaylist Image\tMobile Image\tStreamingLink");

                    foreach (var showId in showList)
                    {
                        var showAsCategory = context.CategoryClasses.Find(showId);

                        if (showAsCategory is Show)
                        {
                            var show = (Show)showAsCategory;
                            outfile.WriteLine("{0}\t{1}\t{2}", show.CategoryId, show.CategoryName, show.Episodes.Count());
                            foreach (var ep in show.Episodes.OrderBy(ep => ep.Episode.DateAired))
                            {
                                var hlsAssets = from a in ep.Episode.PremiumAssets
                                                from ac in a.Asset.AssetCdns
                                                where ac.CdnId == 2
                                                select ac;

                                var hlsUrl = hlsAssets.Count() == 0 ? "" : hlsAssets.FirstOrDefault().CdnReference;

                                outfile.Write("{0}\t{1}\t{2}", "", "", "");
                                outfile.WriteLine("\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", ep.Episode.OnlineStatusId, ep.Episode.DateAired, ep.Episode.EpisodeName, ep.Episode.EpisodeNumber, ep.Episode.ImageAssets.ImageVideo, ep.Episode.ImageAssets.ImagePlaylist, ep.Episode.ImageAssets.ImageMobile, hlsUrl);
                            }
                        }

                    }
                }

            }

        }
    }
}
