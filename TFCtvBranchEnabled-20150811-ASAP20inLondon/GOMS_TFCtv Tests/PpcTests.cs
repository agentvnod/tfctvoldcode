using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IPTV2_Model;

namespace GOMS_TFCtv_Tests
{
    class PpcTests
    {
        public static void LoadUatTrialPpcs()
        {
            var ppcPath = @"C:\Users\epaden\Desktop\TFCTV Technical Documentation\Extracted Pins\";
            LoadPpc(ppcPath + "tfbjl0000001-tfbjl0000020.txt");
            LoadPpc(ppcPath + "tgbjl0000001-tgbjl0000020.txt");
            LoadPpc(ppcPath + "thbjl0000001-thbjl0000020.txt");

        }

        public static void LoadMoreUatPpcs()
        {
            var ppcPath = @"C:\Users\epaden\Desktop\TFCTV Technical Documentation\UAT Pins\";
            LoadPpc(ppcPath + "abbtl0000001-abbtl0000020.txt");
            LoadPpc(ppcPath + "acbtl0000001-acbtl0000020.txt");
            LoadPpc(ppcPath + "sbbtl0000001-sbbtl0000020.txt");
            LoadPpc(ppcPath + "scbtl0000001-scbtl0000020.txt");
            LoadPpc(ppcPath + "abbql0000001-abbql0000040.txt");
            LoadPpc(ppcPath + "acbql0000001-acbql0000040.txt");

        }

        public static void LoadUatPpcs()
        {
            var ppcPath = @"C:\Users\epaden\Desktop\TFCTV Technical Documentation\Extracted Pins\";
            LoadPpc(ppcPath + "aabjl0000001-aabjl0000020.txt");
            LoadPpc(ppcPath + "abbjl0000001-abbjl0000020.txt");
            LoadPpc(ppcPath + "acbjl0000001-acbjl0000020.txt");
            LoadPpc(ppcPath + "adbjl0000001-adbjl0000020.txt");
            LoadPpc(ppcPath + "aebjl0000001-aebjl0000020.txt");
            LoadPpc(ppcPath + "babjl0000001-babjl0000020.txt");
            LoadPpc(ppcPath + "bbbjl0000001-bbbjl0000020.txt");
            LoadPpc(ppcPath + "bcbjl0000001-bcbjl0000020.txt");
            LoadPpc(ppcPath + "bdbjl0000001-bdbjl0000020.txt");
            LoadPpc(ppcPath + "bebjl0000001-bebjl0000020.txt");
            LoadPpc(ppcPath + "cabjl0000001-cabjl0000020.txt");
            LoadPpc(ppcPath + "cbbjl0000001-cbbjl0000020.txt");
            LoadPpc(ppcPath + "ccbjl0000001-ccbjl0000020.txt");
            LoadPpc(ppcPath + "cdbjl0000001-cdbjl0000020.txt");
            LoadPpc(ppcPath + "cebjl0000001-cebjl0000020.txt");
            LoadPpc(ppcPath + "dabjl0000001-dabjl0000020.txt");
            LoadPpc(ppcPath + "dbbjl0000001-dbbjl0000020.txt");
            LoadPpc(ppcPath + "dcbjl0000001-dcbjl0000020.txt");
            LoadPpc(ppcPath + "ddbjl0000001-ddbjl0000020.txt");
            LoadPpc(ppcPath + "debjl0000001-debjl0000020.txt");
            LoadPpc(ppcPath + "eabjl0000001-eabjl0000020.txt");
            LoadPpc(ppcPath + "ebbjl0000001-ebbjl0000020.txt");
            LoadPpc(ppcPath + "ecbjl0000001-ecbjl0000020.txt");
            LoadPpc(ppcPath + "edbjl0000001-edbjl0000020.txt");
            LoadPpc(ppcPath + "eebjl0000001-eebjl0000020.txt");
            LoadPpc(ppcPath + "fabjl0000001-fabjl0000020.txt");
            LoadPpc(ppcPath + "fbbjl0000001-fbbjl0000020.txt");
            LoadPpc(ppcPath + "fcbjl0000001-fcbjl0000020.txt");
            LoadPpc(ppcPath + "fdbjl0000001-fdbjl0000020.txt");
            LoadPpc(ppcPath + "febjl0000001-febjl0000020.txt");
            LoadPpc(ppcPath + "gabjl0000001-gabjl0000020.txt");
            LoadPpc(ppcPath + "gbbjl0000001-gbbjl0000020.txt");
            LoadPpc(ppcPath + "gcbjl0000001-gcbjl0000020.txt");
            LoadPpc(ppcPath + "gdbjl0000001-gdbjl0000020.txt");
            LoadPpc(ppcPath + "gebjl0000001-gebjl0000020.txt");
            LoadPpc(ppcPath + "habjl0000001-habjl0000020.txt");
            LoadPpc(ppcPath + "hbbjl0000001-hbbjl0000020.txt");
            LoadPpc(ppcPath + "hcbjl0000001-hcbjl0000020.txt");
            LoadPpc(ppcPath + "hdbjl0000001-hdbjl0000020.txt");
            LoadPpc(ppcPath + "hebjl0000001-hebjl0000020.txt");
            LoadPpc(ppcPath + "jabjl0000001-jabjl0000020.txt");
            LoadPpc(ppcPath + "jbbjl0000001-jbbjl0000020.txt");
            LoadPpc(ppcPath + "jcbjl0000001-jcbjl0000020.txt");
            LoadPpc(ppcPath + "jdbjl0000001-jdbjl0000020.txt");
            LoadPpc(ppcPath + "jebjl0000001-jebjl0000020.txt");
            LoadPpc(ppcPath + "kabjl0000001-kabjl0000020.txt");
            LoadPpc(ppcPath + "kbbjl0000001-kbbjl0000020.txt");
            LoadPpc(ppcPath + "kcbjl0000001-kcbjl0000020.txt");
            LoadPpc(ppcPath + "kdbjl0000001-kdbjl0000020.txt");
            LoadPpc(ppcPath + "kebjl0000001-kebjl0000020.txt");
            LoadPpc(ppcPath + "labjl0000001-labjl0000020.txt");
            LoadPpc(ppcPath + "lbbjl0000001-lbbjl0000020.txt");
            LoadPpc(ppcPath + "lcbjl0000001-lcbjl0000020.txt");
            LoadPpc(ppcPath + "ldbjl0000001-ldbjl0000020.txt");
            LoadPpc(ppcPath + "lebjl0000001-lebjl0000020.txt");
            LoadPpc(ppcPath + "mabjl0000001-mabjl0000020.txt");
            LoadPpc(ppcPath + "mbbjl0000001-mbbjl0000020.txt");
            LoadPpc(ppcPath + "mcbjl0000001-mcbjl0000020.txt");
            LoadPpc(ppcPath + "mdbjl0000001-mdbjl0000020.txt");
            LoadPpc(ppcPath + "mebjl0000001-mebjl0000020.txt");
            LoadPpc(ppcPath + "nabjl0000001-nabjl0000020.txt");
            LoadPpc(ppcPath + "nbbjl0000001-nbbjl0000020.txt");
            LoadPpc(ppcPath + "ncbjl0000001-ncbjl0000020.txt");
            LoadPpc(ppcPath + "ndbjl0000001-ndbjl0000020.txt");
            LoadPpc(ppcPath + "nebjl0000001-nebjl0000020.txt");
            LoadPpc(ppcPath + "oabjl0000001-oabjl0000020.txt");
            LoadPpc(ppcPath + "obbjl0000001-obbjl0000020.txt");
            LoadPpc(ppcPath + "ocbjl0000001-ocbjl0000020.txt");
            LoadPpc(ppcPath + "odbjl0000001-odbjl0000020.txt");
            LoadPpc(ppcPath + "oebjl0000001-oebjl0000020.txt");
            LoadPpc(ppcPath + "pabjl0000001-pabjl0000020.txt");
            LoadPpc(ppcPath + "pbbjl0000001-pbbjl0000020.txt");
            LoadPpc(ppcPath + "pcbjl0000001-pcbjl0000020.txt");
            LoadPpc(ppcPath + "pdbjl0000001-pdbjl0000020.txt");
            LoadPpc(ppcPath + "pebjl0000001-pebjl0000020.txt");
            LoadPpc(ppcPath + "qabjl0000001-qabjl0000020.txt");
            LoadPpc(ppcPath + "qbbjl0000001-qbbjl0000020.txt");
            LoadPpc(ppcPath + "qcbjl0000001-qcbjl0000020.txt");
            LoadPpc(ppcPath + "qdbjl0000001-qdbjl0000020.txt");
            LoadPpc(ppcPath + "qebjl0000001-qebjl0000020.txt");
            LoadPpc(ppcPath + "rabjl0000001-rabjl0000020.txt");
            LoadPpc(ppcPath + "rbbjl0000001-rbbjl0000020.txt");
            LoadPpc(ppcPath + "rcbjl0000001-rcbjl0000020.txt");
            LoadPpc(ppcPath + "rdbjl0000001-rdbjl0000020.txt");
            LoadPpc(ppcPath + "rebjl0000001-rebjl0000020.txt");
            LoadPpc(ppcPath + "sabjl0000001-sabjl0000020.txt");
            LoadPpc(ppcPath + "sbbjl0000001-sbbjl0000020.txt");
            LoadPpc(ppcPath + "scbjl0000001-scbjl0000020.txt");
            LoadPpc(ppcPath + "sdbjl0000001-sdbjl0000020.txt");
            LoadPpc(ppcPath + "sebjl0000001-sebjl0000020.txt");
        }

        public static void LoadPpc(string fileName)
        {
            var context = new IPTV2Entities();

            using (StreamReader fileReader = new StreamReader(fileName))
            {
                Ppc.Import(context, fileReader);
                //var firstLine = fileReader.ReadLine();
                //var firstLineSplit = firstLine.Split('|');
                //var ppcType = context.PpcTypes.Find(Convert.ToInt32(firstLineSplit[0]));

                //if (ppcType != null)
                //{
                //    while (!fileReader.EndOfStream)
                //    {
                //        var ppcLine = fileReader.ReadLine();
                //        var ppcLineSplit = ppcLine.Split('|');
                //        if (firstLineSplit[0] != ppcLineSplit[0])
                //        {
                //            throw new Exception("PPC file inconsistent.");
                //        }

                //        if (ppcLineSplit[1].StartsWith(ppcType.PpcProductCode))
                //        {


                //            if (ppcType is ReloadPpcType)
                //            {
                //                var reloadPpc = new ReloadPpc
                //                {
                //                    PpcType = ppcType,
                //                    SerialNumber = ppcLineSplit[1],
                //                    Pin = ppcLineSplit[2],
                //                    ExpirationDate = DateTime.Parse(ppcLineSplit[3]),
                //                    Amount = ppcType.Amount,
                //                    Currency = ppcType.Currency
                //                };
                //                context.Ppcs.Add(reloadPpc);
                //            }
                //            else if (ppcType is SubscriptionPpcType)
                //            {
                //                var subscriptionPpcType = (SubscriptionPpcType)ppcType;
                //                var subscriptionPpc = new SubscriptionPpc
                //                {
                //                    PpcType = subscriptionPpcType,
                //                    SerialNumber = ppcLineSplit[1],
                //                    Pin = ppcLineSplit[2],
                //                    ExpirationDate = DateTime.Parse(ppcLineSplit[3]),
                //                    Amount = subscriptionPpcType.Amount,
                //                    Currency = subscriptionPpcType.Currency,
                //                    Duration = subscriptionPpcType.Duration,
                //                    DurationType = subscriptionPpcType.DurationType,
                //                    ProductId = subscriptionPpcType.ProductId
                //                };
                //                context.Ppcs.Add(subscriptionPpc);

                //            }
                //            else
                //            {
                //                throw new Exception("invalid PPC type");
                //            }
                //        }
                //        else
                //        {
                //            throw new Exception("Invalid PPC file.");
                //        }
                //    }
                //    context.SaveChanges();
                //}
                //else
                //{
                //    throw new Exception("invalid PPC type.");
                //}
            }
        }

        public static void ExportAllPpcs()
        {
            ExportPpc("AABJL0000001", "AABJL0000020");
            ExportPpc("ABBJL0000001", "ABBJL0000020");
            ExportPpc("ABBQL0000001", "ABBQL0000040");
            ExportPpc("ABBTL0000001", "ABBTL0000020");
            ExportPpc("ACBJL0000001", "ACBJL0000020");
            ExportPpc("ACBQL0000001", "ACBQL0000040");
            ExportPpc("ACBTL0000001", "ACBTL0000020");
            ExportPpc("ADBJL0000001", "ADBJL0000020");
            ExportPpc("AEBJL0000001", "AEBJL0000020");
            ExportPpc("BABJL0000001", "BABJL0000020");
            ExportPpc("BBBJL0000001", "BBBJL0000020");
            ExportPpc("BCBJL0000001", "BCBJL0000020");
            ExportPpc("BDBJL0000001", "BDBJL0000020");
            ExportPpc("BEBJL0000001", "BEBJL0000020");
            ExportPpc("CABJL0000001", "CABJL0000020");
            ExportPpc("CBBJL0000001", "CBBJL0000020");
            ExportPpc("CCBJL0000001", "CCBJL0000020");
            ExportPpc("CDBJL0000001", "CDBJL0000020");
            ExportPpc("CEBJL0000001", "CEBJL0000020");
            ExportPpc("DABJL0000001", "DABJL0000020");
            ExportPpc("DBBJL0000001", "DBBJL0000020");
            ExportPpc("DCBJL0000001", "DCBJL0000020");
            ExportPpc("DDBJL0000001", "DDBJL0000020");
            ExportPpc("DEBJL0000001", "DEBJL0000020");
            ExportPpc("EABJL0000001", "EABJL0000020");
            ExportPpc("EBBJL0000001", "EBBJL0000020");
            ExportPpc("ECBJL0000001", "ECBJL0000020");
            ExportPpc("EDBJL0000001", "EDBJL0000020");
            ExportPpc("EEBJL0000001", "EEBJL0000020");
            ExportPpc("FABJL0000001", "FABJL0000020");
            ExportPpc("FBBJL0000001", "FBBJL0000020");
            ExportPpc("FCBJL0000001", "FCBJL0000020");
            ExportPpc("FDBJL0000001", "FDBJL0000020");
            ExportPpc("FEBJL0000001", "FEBJL0000020");
            ExportPpc("GABJL0000001", "GABJL0000020");
            ExportPpc("GBBJL0000001", "GBBJL0000020");
            ExportPpc("GCBJL0000001", "GCBJL0000020");
            ExportPpc("GDBJL0000001", "GDBJL0000020");
            ExportPpc("GEBJL0000001", "GEBJL0000020");
            ExportPpc("HABJL0000001", "HABJL0000020");
            ExportPpc("HBBJL0000001", "HBBJL0000020");
            ExportPpc("HCBJL0000001", "HCBJL0000020");
            ExportPpc("HDBJL0000001", "HDBJL0000020");
            ExportPpc("HEBJL0000001", "HEBJL0000020");
            ExportPpc("JABJL0000001", "JABJL0000020");
            ExportPpc("JBBJL0000001", "JBBJL0000020");
            ExportPpc("JCBJL0000001", "JCBJL0000020");
            ExportPpc("JDBJL0000001", "JDBJL0000020");
            ExportPpc("JEBJL0000001", "JEBJL0000020");
            ExportPpc("KABJL0000001", "KABJL0000020");
            ExportPpc("KBBJL0000001", "KBBJL0000020");
            ExportPpc("KCBJL0000001", "KCBJL0000020");
            ExportPpc("KDBJL0000001", "KDBJL0000020");
            ExportPpc("KEBJL0000001", "KEBJL0000020");
            ExportPpc("LABJL0000001", "LABJL0000020");
            ExportPpc("LBBJL0000001", "LBBJL0000020");
            ExportPpc("LCBJL0000001", "LCBJL0000020");
            ExportPpc("LDBJL0000001", "LDBJL0000020");
            ExportPpc("LEBJL0000001", "LEBJL0000020");
            ExportPpc("MABJL0000001", "MABJL0000020");
            ExportPpc("MBBJL0000001", "MBBJL0000020");
            ExportPpc("MCBJL0000001", "MCBJL0000020");
            ExportPpc("MDBJL0000001", "MDBJL0000020");
            ExportPpc("MEBJL0000001", "MEBJL0000020");
            ExportPpc("NABJL0000001", "NABJL0000020");
            ExportPpc("NBBJL0000001", "NBBJL0000020");
            ExportPpc("NCBJL0000001", "NCBJL0000020");
            ExportPpc("NDBJL0000001", "NDBJL0000020");
            ExportPpc("NEBJL0000001", "NEBJL0000020");
            ExportPpc("OABJL0000001", "OABJL0000020");
            ExportPpc("OBBJL0000001", "OBBJL0000020");
            ExportPpc("OCBJL0000001", "OCBJL0000020");
            ExportPpc("ODBJL0000001", "ODBJL0000020");
            ExportPpc("OEBJL0000001", "OEBJL0000020");
            ExportPpc("PABJL0000001", "PABJL0000020");
            ExportPpc("PBBJL0000001", "PBBJL0000020");
            ExportPpc("PCBJL0000001", "PCBJL0000020");
            ExportPpc("PDBJL0000001", "PDBJL0000020");
            ExportPpc("PEBJL0000001", "PEBJL0000020");
            ExportPpc("QABJL0000001", "QABJL0000020");
            ExportPpc("QBBJL0000001", "QBBJL0000020");
            ExportPpc("QCBJL0000001", "QCBJL0000020");
            ExportPpc("QDBJL0000001", "QDBJL0000020");
            ExportPpc("QEBJL0000001", "QEBJL0000020");
            ExportPpc("RABJL0000001", "RABJL0000020");
            ExportPpc("RBBJL0000001", "RBBJL0000020");
            ExportPpc("RCBJL0000001", "RCBJL0000020");
            ExportPpc("RDBJL0000001", "RDBJL0000020");
            ExportPpc("REBJL0000001", "REBJL0000020");
            ExportPpc("SABJL0000001", "SABJL0000020");
            ExportPpc("SBBJL0000001", "SBBJL0000020");
            ExportPpc("SBBTL0000001", "SBBTL0000020");
            ExportPpc("SCBJL0000001", "SCBJL0000020");
            ExportPpc("SCBTL0000001", "SCBTL0000020");
            ExportPpc("SDBJL0000001", "SDBJL0000020");
            ExportPpc("SEBJL0000001", "SEBJL0000020");
            ExportPpc("TFBJL0000001", "TFBJL0000020");
            ExportPpc("TGBJL0000001", "TGBJL0000020");
            ExportPpc("THBJL0000001", "THBJL0000020");
        }

        public static void ExportPpc(String startSerial, String endSerial)
        {
            var context = new IPTV2Entities();
            string fileName = @"C:\Users\epaden\Desktop\TFCTV Technical Documentation\Extracted Pins\PpcGomsExport-" + startSerial + "-" + endSerial + ".txt";

            using (StreamWriter fileWriter = new StreamWriter(fileName))
            {
                Ppc.GomsExport(context, fileWriter, startSerial, endSerial);
            }
        }

        static public void GomsExportAll()
        {
            var context = new IPTV2Entities();
            string fileName = @"C:\Users\epaden\Desktop\TFCTV Technical Documentation\Extracted Pins\PpcGomsExport-All.txt";
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                streamWriter.WriteLine("Serial Number,Product Type,Pins,Subsidiary,Denomination,Currency,Expiry Date,Status,Location");
                // var ppcList = context.Ppcs.Where(p => (p.SerialNumber.CompareTo(startSerial) >= 0) && (p.SerialNumber.CompareTo(endSerial) <= 0));
                foreach (var thisPpc in context.Ppcs)
                {
                    string amount = "";
                    string currency = "";
                    if (thisPpc is ReloadPpc)
                    {
                        amount = thisPpc.Amount.ToString();
                        currency = thisPpc.PpcType.CurrencyReference.Name;
                    }
                    streamWriter.Write(thisPpc.SerialNumber);
                    streamWriter.Write(",");
                    streamWriter.Write(thisPpc.PpcType.Description);
                    streamWriter.Write(",");
                    streamWriter.Write(thisPpc.Pin);
                    streamWriter.Write(",");
                    streamWriter.Write(thisPpc.PpcType.GomsSubsidiary.Code);
                    streamWriter.Write(",");
                    streamWriter.Write(amount);
                    streamWriter.Write(",");
                    streamWriter.Write(currency);
                    streamWriter.Write(",");
                    streamWriter.Write(thisPpc.ExpirationDate.ToShortDateString());
                    streamWriter.Write(",");
                    streamWriter.Write("Enabled");
                    streamWriter.Write(",");
                    streamWriter.WriteLine();
                }
            }
        }
    }
}
