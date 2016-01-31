using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using VideoEngagementsModel;
using System.Data.SqlClient;
using System.Configuration;

namespace VideoEngagementsTableCreator
{
    class VideoEngagementsTable : IJob
    {
        static int addMonths = Convert.ToInt32(ConfigurationManager.AppSettings["addMonths"]);

        public VideoEngagementsTable() { }

        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Table creation is executing. Time now is " + DateTime.UtcNow);
            Send();
        }

        private void Send()
        {
            //Get Date (Year) Today

            var registYear = DateTime.Now.AddMonths(addMonths).Year;
            //Loop from 01 to 12
            try
            {
                using (var context = new VideoEngagementsEntities())
                {
                    for (int i = 0; i < 12; i++)
                    {
                        var returnCode = new SqlParameter()
                        {
                            ParameterName = "returnCode",
                            DbType = System.Data.DbType.Int32,
                            Direction = System.Data.ParameterDirection.Output
                        };

                        string tableName = String.Format("EpisodePlay{0}{1:00}", registYear, i + 1);
                        var result = context.Database.ExecuteSqlCommand("EXEC CreateLogTable @table, @returnCode OUTPUT",
                                        new object[] {  
                                 new SqlParameter("table", tableName),                                 
                                 returnCode
                                });

                        if ((int)returnCode.Value > 0)
                            Console.WriteLine(String.Format("Table {0} has been successfully created.", tableName));
                        else
                            if ((int)returnCode.Value == -1)
                                Console.WriteLine(String.Format("Table {0} already exists. Skipping...", tableName));
                            else
                                Console.WriteLine(String.Format("Table {0} creation failed.", tableName));
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(String.Format("Error: {0}", e.Message)); }
        }
    }
}
