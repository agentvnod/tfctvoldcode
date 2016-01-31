using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;
using Elmah;
using System.Collections;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Microsoft.WindowsAzure.Storage;

namespace TFCTV
{
    public class ErrorEntity : TableServiceEntity
    {
        public string SerializedError { get; set; }

        public ErrorEntity() { }
        public ErrorEntity(Error error)
            : base(string.Empty, (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"))
        {
            this.SerializedError = ErrorXml.EncodeString(error);
        }
    }

    public class TableErrorLog : ErrorLog
    {
        private string connectionString;
        private string tableName = TFCTV.Helpers.Settings.GetSettings("ElmahTableName");

        public override ErrorLogEntry GetError(string id)
        {
            return new ErrorLogEntry(this, id, ErrorXml.DecodeString(CloudStorageAccount.Parse(connectionString).CreateCloudTableClient().GetTableServiceContext().CreateQuery<ErrorEntity>(tableName).Where(e => e.PartitionKey == string.Empty && e.RowKey == id).Single().SerializedError));
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            var count = 0;
            foreach (var error in CloudStorageAccount.Parse(connectionString).CreateCloudTableClient().GetTableServiceContext().CreateQuery<ErrorEntity>(tableName).Where(e => e.PartitionKey == string.Empty).AsQueryable().Take((pageIndex + 1) * pageSize).ToList().Skip(pageIndex * pageSize))
            {
                errorEntryList.Add(new ErrorLogEntry(this, error.RowKey, ErrorXml.DecodeString(error.SerializedError)));
                count += 1;
            }
            return count;
        }

        public override string Log(Error error)
        {
            var entity = new ErrorEntity(error);
            var context = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient().GetTableServiceContext();
            context.AddObject(tableName, entity);
            context.SaveChangesWithRetries();
            return entity.RowKey;
        }

        public TableErrorLog(IDictionary config)
        {
            //connectionString = (string)config["connectionString"] ?? RoleEnvironment.GetConfigurationSettingValue((string)config["connectionStringName"]);
            connectionString = TFCTV.Helpers.Settings.GetSettings("TfcTvInternalStorage");
            Initialize();
        }

        public TableErrorLog(string connectionString)
        {
            this.connectionString = connectionString;
            Initialize();
        }

        void Initialize()
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var cloudTable = tableClient.GetTableReference(tableName);
            cloudTable.CreateIfNotExists();
        }
    }

}