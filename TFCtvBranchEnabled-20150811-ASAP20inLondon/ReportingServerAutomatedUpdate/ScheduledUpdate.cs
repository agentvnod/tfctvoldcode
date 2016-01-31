using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Threading;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;
using SendGridMail;
using SendGridMail.Transport;

namespace ReportingServerAutomatedUpdate
{
    public class ScheduledUpdate : IJob
    {
        public ScheduledUpdate() { }
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Reporting Server Update is starting. Time now is " + DateTime.UtcNow);
            run();
        }
        public void run()
        {
            try
            {
                {
                    Console.WriteLine("Daily Report Started");
                    doDailyReport();
                    Console.WriteLine("Daily Report Completed and Sent");

                    string u = "Users";
                    string t = "Transactions";
                    string er = "EntitlementRequests";
                    string p = "Purchases";
                    string pi = "PurchaseItems";
                    string pr = "Products";
                    string uField = "registrationdate";
                    string tField = "transactionid";
                    string erField = "daterequested";
                    string pField = "purchaseid";
                    string piField = "purchaseitemid";
                    string prField = "Productid";
                    doBCPQueryOut(u, uField, "date");
                    doBCPQueryOut(t, tField, "id");
                    doBCPQueryOut(er, erField, "date");
                    doBCPQueryOut(p, pField, "id");
                    doBCPQueryOut(pi, piField, "id");
                    doBCPQueryOut(pr, prField, "id");
                    Process process = new Process();
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;

                    
                    doBCPCopy(u);
                    doBCPCopy(p);
                    doBCPCopy(pi);
                    doBCPCopy(er);
                    doBCPCopy(t);
                    doBCPCopy(pr);
                    Console.WriteLine("BCP Update ALL Complete");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public class ParamArray
        {
            string table;
            string field;
            string fieldtype;

        }
        static void doBCPQueryOut(string pTable, string pField, string pFieldType)
        {
            Process process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            string sourceServer = ConfigurationManager.AppSettings["sourceServer"].ToString();
            string sourceDB = ConfigurationManager.AppSettings["sourceDB"].ToString();
            string sourceUser = ConfigurationManager.AppSettings["sourceUser"].ToString();
            string sourcePass = ConfigurationManager.AppSettings["sourcePass"].ToString();
            string directory = ConfigurationManager.AppSettings["directory"].ToString();
            if (pFieldType == "date")
            {
                string dateParam = getmaxdateparam(pTable, pField).ToString("yyyy-MM-dd HH:mm:ss.fff");
                Console.WriteLine("maxdate: " + dateParam);

                process.StartInfo.FileName = "bcp";
                //process.StartInfo.Arguments = @"select * from users where registrationdate > '"+dateParam+@"' queryout D:\dbtransfer\users.txt -d TFCtv -c -U amadolb@ -S tcp: -P <add key="SendGridSmtpPort" value=""/>";
                process.StartInfo.Arguments = "\"select * from " + pTable + " where " + pField + " > '" + dateParam + "'\" queryout " + directory + pTable + ".txt -d " + sourceDB + " -c -U " + sourceUser + " -S " + sourceServer + " -P " + sourcePass + " ";

                Console.WriteLine(pTable + " bcp out starting.");

                process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                process.Start();

                process.BeginOutputReadLine();

                process.WaitForExit();
                Console.WriteLine("DONE!");
            }
            else if (pFieldType == "id")
            {
                process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                int idParam = getmaxidparam(pTable, pField); ;
                Console.WriteLine("maxid: " + idParam);

                process.StartInfo.FileName = "bcp";
                //process.StartInfo.Arguments = "\"select * from purchases where date > '" + dateParam + "'\" queryout D:\dbtransfer\purchases.txt -d TFCtv -c -U amadolb@ -S tcp: -P <add key="SendGridSmtpPort" value=""/>";
                process.StartInfo.Arguments = "\"select * from " + pTable + " where " + pField + " > '" + idParam.ToString() + "'\" queryout " + directory + pTable + ".txt -d " + sourceDB + " -c -U " + sourceUser + " -S " + sourceServer + " -P " + sourcePass + " ";
                process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                Console.WriteLine(pTable + " bcp out starting.");
                process.Start();


                process.BeginOutputReadLine();

                process.WaitForExit();
                Console.WriteLine("DONE!");
            }
        }
        static void doBCPCopy(string table)
        {
            string destinationServer = ConfigurationManager.AppSettings["destinationServer"].ToString();
            string destinationDB = ConfigurationManager.AppSettings["destinationDB"].ToString();
            string destinationUser = ConfigurationManager.AppSettings["destinationUser"].ToString();
            string destinationPass = ConfigurationManager.AppSettings["destinationPass"].ToString();
            string directory = ConfigurationManager.AppSettings["directory"].ToString();
            string strIn = destinationDB + ".dbo." + table + " in " + directory + table + ".txt -c -U " + destinationUser + " -S " + destinationServer + " -P " + destinationPass + " -E";

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "bcp";

            //process.StartInfo.Arguments = "bcp TfctvBackup.dbo.users in D:\\dbtransfer\\users.txt -c -U  -S tcp:pxz3fypo7u.database.windows.net -P ";
            process.StartInfo.Arguments = strIn;
            Console.WriteLine(table + " bcp in starting.");

            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.Start();

            process.BeginOutputReadLine();

            process.WaitForExit();
            Console.WriteLine("DONE!");
        }

        static DateTime getmaxdateparam(string table, string field)
        {
            string destinationServer = ConfigurationManager.AppSettings["destinationServer"].ToString();
            string destinationDB = ConfigurationManager.AppSettings["destinationDB"].ToString();
            string destinationUser = ConfigurationManager.AppSettings["destinationUser"].ToString();
            string destinationPass = ConfigurationManager.AppSettings["destinationPass"].ToString();
            string directory = ConfigurationManager.AppSettings["directory"].ToString();
            //string strCon = "Server=tcp:pxz3fypo7u.database.windows.net;Database=TFCtvBackUp;User ID=;Password=;Trusted_Connection=False;Encrypt=True;Connection Timeout=600;";
            string teststrCon = @"Server=" + destinationServer + ";Database=" + destinationDB + ";User ID=" + destinationUser + ";Password=" + destinationPass + ";Connection Timeout=600;";
            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            con.ConnectionString = teststrCon;
            con.Open();
            cmd.CommandTimeout = 1200;
            cmd.CommandText = @"select max(" + field + ") from " + table;
            cmd.Connection = con;
            DateTime result = (DateTime)cmd.ExecuteScalar();
            con.Close();
            return result;
        }
        static Int32 getmaxidparam(string table, string field)
        {
            string destinationServer = ConfigurationManager.AppSettings["destinationServer"].ToString();
            string destinationDB = ConfigurationManager.AppSettings["destinationDB"].ToString();
            string destinationUser = ConfigurationManager.AppSettings["destinationUser"].ToString();
            string destinationPass = ConfigurationManager.AppSettings["destinationPass"].ToString();
            string directory = ConfigurationManager.AppSettings["directory"].ToString();
            //string strCon = "Server=tcp:pxz3fypo7u.database.windows.net;Database=TFCtvBackUp;User ID=;Password=;Trusted_Connection=False;Encrypt=True;Connection Timeout=600;";
            string teststrCon = @"Server=" + destinationServer + ";Database=" + destinationDB + ";User ID=" + destinationUser + ";Password=" + destinationPass + ";Connection Timeout=600;";
            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            con.ConnectionString = teststrCon;
            con.Open();
            cmd.CommandTimeout = 1200;
            cmd.CommandText = @"select max(" + field + ") from " + table;
            cmd.Connection = con;
            Int32 result = (Int32)cmd.ExecuteScalar();
            con.Close();
            return result;
        }

        public static void doDailyReport()
        {
            string strDailyRegional = ConfigurationManager.AppSettings["dailyconstring"];
            string strDailyEngage = ConfigurationManager.AppSettings["engageconstring"];
            string strDailyVidEngage = ConfigurationManager.AppSettings["videngageconstring"];


            SqlConnection con = new SqlConnection();


            DateTime lastEnteredDate; //getLastEnteredDate()
            DateTime currentDate = DateTime.Now;

            DateTime traverserDate;
            DataSet dsResult = new DataSet();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1200;

            string region = string.Empty;

            try
            {
                //SUBS
                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                Workbooks temp = xlApp.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook xlWorkbook = temp.Open(@"d:\autodaily\daily.xlsx");
                con.ConnectionString = strDailyRegional;
                con.Open();
                //xlApp.Visible = true;
                foreach (Worksheet xlSheet in xlWorkbook.Worksheets)
                {
                    region = xlSheet.Name;
                    Console.WriteLine(region);
                    if (region != "CONSO")
                    {
                        dsResult = new DataSet();
                        Range r = getLastDate(xlSheet);
                        lastEnteredDate = r.Value;
                        //Console.Write(r.Address.ToString());
                        traverserDate = lastEnteredDate.AddDays(1);
                        while (traverserDate.Date < currentDate.Date)
                        {

                            cmd.CommandText = @"select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct u.userid),0) as 'numbers' from users u inner join country c on u.countrycode=c.code inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid where cast(registrationdate as date) = '" + traverserDate.Date.ToString("yyyy-MM-dd") + @"'  and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct er.userid),0) as 'numbers' from entitlementrequests er  inner join users u on u.userid=er.userid inner join country c on u.countrycode=c.code inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where er.productid  in (1,3,4,5,6,7,8,9) and er.source <>'TFC.tv Everywhere' and cast(er.enddate as date)>='" + traverserDate.AddDays(1).ToString("yyyy-MM-dd") + @"'  and er.daterequested<='" + traverserDate.AddDays(1).ToString("yyyy-MM-dd") + @"'  and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct er.userid),0) as 'numbers' from entitlementrequests er  inner join users u on u.userid=er.userid inner join country c on u.countrycode=c.code inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where er.productid  in (1,3,4,5,6,7,8,9) and er.source <>'TFC.tv Everywhere' and cast(er.enddate as date)='" + traverserDate.ToString("yyyy-MM-dd") + @"'  and er.daterequested<='" + traverserDate.AddDays(1).ToString("yyyy-MM-dd") + @"'  and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct userid),0) as 'numbers' from ( select u.email, er.userid,u.firstname,u.lastname,er.productid,er.enddate,u.countrycode  from entitlementrequests er inner join users u on u.userid=er.userid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left outer join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where er.productid  in (1,3,4,5,6,7,8,9) and er.source <>'TFC.tv Everywhere' and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and cast(er.daterequested as date)= '" + traverserDate.ToString("yyyy-MM-dd") + @"'  and er.UserId not in( select er.userid from entitlementrequests er inner join users u on u.userid=er.userid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left outer join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where er.productid  in (1,3,4,5,6,7,8,9) and er.source <>'TFC.tv Everywhere' and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and cast(er.daterequested as date)< '" + traverserDate.ToString("yyyy-MM-dd") + @"'  ) ) as temp inner join country c on c.code=temp.countrycode inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct userid),0) as 'numbers' from ( select u.email, er.userid,u.firstname,u.lastname,er.productid,er.enddate,u.countrycode  from entitlementrequests er inner join users u on u.userid=er.userid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left outer join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where er.productid  in (1,3,4,5,6,7,8,9) and er.source <>'TFC.tv Everywhere' and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and cast(er.daterequested as date)= '" + traverserDate.ToString("yyyy-MM-dd") + @"'  and er.UserId in( select er.userid from entitlementrequests er inner join users u on u.userid=er.userid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left outer join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where er.productid  in (1,3,4,5,6,7,8,9) and er.source <>'TFC.tv Everywhere' and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and cast(er.daterequested as date)< '" + traverserDate.ToString("yyyy-MM-dd") + @"'  ) ) as temp inner join country c on c.code=temp.countrycode inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct er.userid),0) as 'numbers' from entitlementrequests er inner join users u on u.userid=er.userid inner join country c on c.code=u.countrycode inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid inner join products p on p.productid=er.productid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where (p.alacartesubscriptiontypeid is not null or er.productid=2) and er.source <>'TFC.tv Everywhere' and cast(er.enddate as date)>='" + traverserDate.AddDays(1).ToString("yyyy-MM-dd") + @"'  and er.daterequested<='" + traverserDate.AddDays(1).ToString("yyyy-MM-dd") + @"'  and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct userid),0) as 'numbers' from ( select u.email, er.userid,u.firstname,u.lastname,er.productid,er.enddate,u.countrycode from entitlementrequests er  inner join users u on u.userid=er.userid inner join products p on p.productid=er.productid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left outer join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where (p.alacartesubscriptiontypeid is not null or er.productid=2) and er.source <>'TFC.tv Everywhere' and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and cast(er.daterequested as date)= '" + traverserDate.ToString("yyyy-MM-dd") + @"'  ) as temp inner join country c on c.code=temp.countrycode inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid where case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t union all select case when count(*) =0 then 0 else sum(t.numbers) end from (select isnull(count(distinct er.userid),0) as 'numbers' from entitlementrequests er inner join users u on u.userid=er.userid inner join country c on c.code=u.countrycode inner join gomssubsidiary g on c.gomssubsidiaryid=g.gomssubsidiaryid inner join products p on p.productid=er.productid inner join transactions t on er.userid=t.userid left outer join ppctype ppc on left(t.reference,2)=ppc.ppcproductcode left outer join purchaseitems pu on pu.entitlementrequestid = er.entitlementrequestid where (p.alacartesubscriptiontypeid is not null or er.productid=2) and er.source <>'TFC.tv Everywhere' and cast(er.enddate as date)='" + traverserDate.ToString("yyyy-MM-dd") + @"'  and er.daterequested<='" + traverserDate.AddDays(1).ToString("yyyy-MM-dd") + @"'  and (subscriptionppctypeid is null or subscriptionppctypeid not in (2,3,4)) and case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end = 'GLB : " + region + @"' group by case when g.gomssubsidiaryid=6 or g.gomssubsidiaryid=8 or g.gomssubsidiaryid=9 then 'GLB : EU' else g.code end) as t";

                            cmd.Connection = con;
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd.CommandText, con);
                            adapter.SelectCommand.CommandTimeout = 1200;

                            adapter.Fill(dsResult);
                            int c = 3;
                            xlSheet.Cells[2, r.Column + 1].Value = lastEnteredDate.AddDays(1);

                            foreach (DataRow dr in dsResult.Tables[0].Rows)
                            {
                                xlSheet.Cells[c, r.Column + 1].Value = dr[0].ToString();

                                c++;
                            }


                            xlWorkbook.Save();

                            dsResult = new DataSet();
                            r = getLastDate(xlSheet);
                            lastEnteredDate = r.Value;
                            //Console.Write(r.Address.ToString());
                            traverserDate = lastEnteredDate.AddDays(1);

                        }
                    }


                }
                Worksheet paramsheet = xlWorkbook.Worksheets["CONSO"];
                Range rt = getLastDate(paramsheet);
                Console.WriteLine(rt.Value);
                Range sc = paramsheet.Cells[rt.Row, rt.Column - 7 > 0 ? rt.Column - 7 : 0];
                Range ec = paramsheet.Cells[rt.Row + 8, rt.Column];
                Range param = paramsheet.Range[sc, ec];

                string emailtext = makeSubStatHTML(param, "subs") + "<br>";

                con.Close();
                xlWorkbook.Close();

                xlApp.Quit();
                GC.Collect();

                //VIDEO ENGAGEMENTS

                xlApp = new Microsoft.Office.Interop.Excel.Application();
                temp = xlApp.Workbooks;
                xlWorkbook = temp.Open(@"d:\autodaily\videoengagements.xlsx");

                con = new SqlConnection();
                con.ConnectionString = strDailyVidEngage;
                con.Open();
                //xlApp.Visible = true;
                Worksheet xlSheetVE = xlWorkbook.Worksheets[1];
                dsResult = new DataSet(); Range rVE = getLastDate(xlSheetVE);
                lastEnteredDate = rVE.Value;
                //Console.Write(r.Address.ToString());
                traverserDate = lastEnteredDate.AddDays(1);
                while (traverserDate.Date < currentDate.Date)
                {
                    cmd.CommandText = @"select count(distinct userid) from episodeplay where cast(datetime as date) =  '" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' and playtypeid=1 union all select count(*) from episodeplay where cast(datetime as date) =  '" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' and playtypeid=1";

                    cmd.Connection = con;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd.CommandText, con);
                    adapter.SelectCommand.CommandTimeout = 1200;
                    adapter.Fill(dsResult);
                    int c = 3;
                    xlSheetVE.Cells[2, rVE.Column + 1].Value = lastEnteredDate.AddDays(1);

                    foreach (DataRow dr in dsResult.Tables[0].Rows)
                    {
                        xlSheetVE.Cells[c, rVE.Column + 1].Value = dr[0].ToString();

                        c++;
                    }

                    xlWorkbook.Save();

                    dsResult = new DataSet();
                    rVE = getLastDate(xlSheetVE);
                    lastEnteredDate = rVE.Value;
                    //Console.Write(r.Address.ToString());
                    traverserDate = lastEnteredDate.AddDays(1);

                }


                paramsheet = xlWorkbook.Worksheets["Sheet1"];
                rt = getLastDate(paramsheet);
                Console.WriteLine(rt.Value);
                sc = paramsheet.Cells[rt.Row, rt.Column - 7 > 0 ? rt.Column - 7 : 0];
                ec = paramsheet.Cells[rt.Row + 2, rt.Column];
                param = paramsheet.Range[sc, ec];

                emailtext += makeSubStatHTML(param, "vid") + "<br>";

                con.Close();

                xlWorkbook.Close();
                xlApp.Quit();
                GC.Collect();


                //ENGAGEMENTS
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                temp = xlApp.Workbooks;
                xlWorkbook = temp.Open(@"d:\autodaily\engagements.xlsx");

                con = new SqlConnection();
                con.ConnectionString = strDailyEngage;
                con.Open();
                //xlApp.Visible = true;
                Worksheet xlSheetE = xlWorkbook.Worksheets[1];
                dsResult = new DataSet(); Range rE = getLastDate(xlSheetE);
                lastEnteredDate = rE.Value;
                //Console.Write(r.Address.ToString());
                traverserDate = lastEnteredDate.AddDays(1);
                while (traverserDate.Date < currentDate.Date)
                {
                    cmd.CommandText = @"select count(distinct userid) from (select distinct userid from celebrityreactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select distinct userid from channelreactions where cast(datetime as date)= '" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select distinct userid from episodereactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select distinct userid from showreactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select distinct userid from youtubereactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' ) as e union all select count(*) from ( select reactiontypeid,userid,datetime from celebrityreactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select reactiontypeid,userid,datetime from channelreactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select reactiontypeid,userid,datetime from episodereactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select reactiontypeid,userid,datetime from showreactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' union select reactiontypeid,userid,datetime from youtubereactions where cast(datetime as date)='" + traverserDate.Date.ToString("yyyy-MM-dd") + @"' ) as e group by case when reactiontypeid=2 then 'rating/review' else case when reactiontypeid=3 then 'share' else case when reactiontypeid=12 then 'love' else cast(reactiontypeid as varchar(2)) end end end";
                    cmd.Connection = con;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd.CommandText, con);
                    adapter.SelectCommand.CommandTimeout = 1200;
                    adapter.Fill(dsResult);
                    int c = 3;
                    xlSheetE.Cells[2, rE.Column + 1].Value = lastEnteredDate.AddDays(1);

                    foreach (DataRow dr in dsResult.Tables[0].Rows)
                    {
                        xlSheetE.Cells[c, rE.Column + 1].Value = dr[0].ToString();

                        c++;
                    }

                    xlWorkbook.Save();

                    dsResult = new DataSet();
                    rE = getLastDate(xlSheetE);
                    lastEnteredDate = rE.Value;
                    //Console.Write(r.Address.ToString());
                    traverserDate = lastEnteredDate.AddDays(1);

                }

                paramsheet = xlWorkbook.Worksheets["Sheet1"];
                rt = getLastDate(paramsheet);
                Console.WriteLine(rt.Value);
                sc = paramsheet.Cells[rt.Row, rt.Column - 7 > 0 ? rt.Column - 7 : 0];
                ec = paramsheet.Cells[rt.Row + 4, rt.Column];
                param = paramsheet.Range[sc, ec];

                emailtext += makeSubStatHTML(param, "eng") + "<br>";
                con.Close();

                SendEmailViaSendGrid(emailtext);
                xlWorkbook.Close();
                xlApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(temp);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(param);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlSheetE);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlSheetVE);



                GC.Collect();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            
        }
        static Range getLastDate(Worksheet ws)
        {
            //            int lastCol = ws.Cells.Find("*", System.Reflection.Missing.Value,
            //System.Reflection.Missing.Value, System.Reflection.Missing.Value, Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;


            int i = 2;
            DateTime datedefault = DateTime.Parse("12/30/1899");
            if (ws.Name == "CONSO")
            {
                while (ws.Cells[2, i].Value != datedefault)
                {

                    //cellValue=ws.Cells[2, i].Value;
                    //Console.Write(ws.Cells[2, i].Value);
                    i++;
                }
            }
            else


                while ((ws.Cells[2, i].Value) is DateTime)
                {

                    //cellValue=ws.Cells[2, i].Value;
                    //Console.Write(ws.Cells[2, i].Value);
                    i++;
                }

            Range aRange = ws.Cells[2, i - 1];
            return aRange;
        }

        static string makeSubStatHTML(Range tableRange, string reporttable)
        {
            string cv = "";
            DateTime cd;
            string sTable = "<table border=1;>";
            int i = 0;

            string[] subslabels = { "", "New Registration", "Active Subscription Entitlements", "Expires on the day", "New Entitlements", "Renewed Entitlements", "Active Retail Entitlements", "New Retail Entitlements", "Retails Expiring on the day" };
            string[] vidlabels = { "", "Distinct Users That Played", "Distinct Plays" };
            string[] englabels = { "", "Total Actions on the Day", "Love", "Rating/Review", "Share", "Total Actions on the Day" };
            string[] labels = subslabels;
            if (reporttable == "vid")
                labels = vidlabels;
            else if (reporttable == "eng")
                labels = englabels;

            foreach (Range row in tableRange.Rows)
            {
                sTable += "<tr><td bgcolor=\"#C0C0C0\">" + labels[i] + @"</td>";

                foreach (Range cell in row.Cells)
                {
                    cv = cell.Value.ToString();
                    if (DateTime.TryParse(cv, out cd))
                    {
                        cv = cd.ToString("MM/dd/yyyy");
                        sTable += "<td bgcolor=\"#C0C0C0\">" + cv + "</td>";
                    }
                    else
                        sTable += "<td>" + cv + "</td>";
                }
                sTable += "</tr>";
                i++;
            }
            sTable += "</table>";

            return sTable;
        }
        public static void SendEmailViaSendGrid(string htmlBody)
        {
         string NoReplyEmail = ConfigurationManager.AppSettings["NoReplyEmail"];
         string SendGridUsername = ConfigurationManager.AppSettings["SendGridUsername"];
         string SendGridPassword = ConfigurationManager.AppSettings["SendGridPassword"];
         string SendGridSmtpHost = ConfigurationManager.AppSettings["SendGridSmtpHost"];
         //string to = ConfigurationManager.AppSettings["emailsTo"];
         string to = ConfigurationManager.AppSettings["emailsTo"];
         string from = ConfigurationManager.AppSettings["From"];
         string subject = ConfigurationManager.AppSettings["Subject"];
         string attachment = ConfigurationManager.AppSettings["AttachmentPath"];
         int SendGridSmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SendGridSmtpPort"]);

            try
            {   
                var message = SendGrid.GetInstance();
                    message.AddTo(to.Split(',').ToList());
                
                message.From = new System.Net.Mail.MailAddress(from);
                message.Subject = subject;

                    message.Html = htmlBody;
                    message.AddAttachment(attachment);

                //Dictionary<string, string> collection = new Dictionary<string, string>();
                //collection.Add("header", "header");
                //message.Headers = collection;

                message.EnableOpenTracking();
                message.EnableClickTracking();
                message.DisableUnsubscribe();
                message.DisableFooter();
                message.EnableBypassListManagement();
                var transportInstance = SMTP.GetInstance(new System.Net.NetworkCredential(SendGridUsername, SendGridPassword), SendGridSmtpHost, SendGridSmtpPort);
                transportInstance.Deliver(message);
                Console.WriteLine("SendGrid: Email was sent successfully to " + to);
            }
            catch (Exception)
            {
                Console.WriteLine("SendGrid: Unable to send email to " + to);
                throw;
            }
        }
        //private static void sendemail(string htmlbody)
        //{
        //    SmtpClient client = new SmtpClient();
        //    client.Port = 587;
        //    client.Host = "smtp.gmail.com";
        //    client.EnableSsl = true;
        //    //client.Timeout = 10000;
        //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //    client.UseDefaultCredentials = false;
        //    client.Credentials = new System.Net.NetworkCredential("tfc.tvdailyreporter@gmail.com", "thisisapassword1");

        //    MailMessage mm = new MailMessage("tfc.tvdailyreporter@gmail.com", "Amado_Berces@abs-cbn.com");
        //    mm.To.Add("Albin_Lim@abs-cbn.com,James_Alcantara@abs-cbn.com,John_Tan@abs-cbn.com");

        //    //mm.To.Add("Amado_Berce@abs-cbn.com");
        //    //mm.To.Add(@"Albin_Lim@abs-cbn.com,Franz_Tupaz@abs-cbn.com,Jihan_Pring@abs-cbn.com,James_Alcantara@abs-cbn.com,John_Tan@abs-cbn.com,Sheila_Gamilla@abs-cbn.com,Angelica_Abano@abs-cbn.com,LeslieAnne_Hernandez@abs-cbn.com,Eugene_Paden@abs-cbn.com,Melissa_Jones@abs-cbn.com,Audie_Avecilla-Riola@abs-cbn.com,Amabel_Acosta@abs-cbn.com,LuisMiguel_Ereneta@abs-cbn.com,Regina_Gatchalian@abs-cbn.com,Rebecca_Ramirez@abs-cbn.com,Dennis_Lim@abs-cbn.com");

        //    Attachment dailyfile = new Attachment(@"d:\autodaily\daily.xlsx");
        //    mm.Attachments.Add(dailyfile);
        //    mm.BodyEncoding = UTF8Encoding.UTF8;
        //    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        //    mm.IsBodyHtml = true;
        //    mm.Body = htmlbody;
        //    client.Send(mm);

        //}
    }
}
