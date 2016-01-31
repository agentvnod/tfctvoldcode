using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace TFCTV.Helpers
{
    public class AzureStorage
    {
        protected static CloudStorageAccount _storageAccount;
        protected static CloudBlobClient _blobClient;
        protected static LocalResource _cloudDriveLocalCache;

        // const string DCACHE_NAME = "DriveCacheName";
        const string DCACHE_LOCATION = "cache";
        const string STORAGE_ACCOUNT_SETTING = "TfcTvInternalStorage";

        static AzureStorage()
        {            
            //  _storageAccount.Credentials = credentials;
            //CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) => configSetter(Settings.GetSettings(configName)));
            //_storageAccount = CloudStorageAccount.FromConfigurationSetting(STORAGE_ACCOUNT_SETTING);
            _storageAccount = CloudStorageAccount.Parse(Settings.GetSettings(STORAGE_ACCOUNT_SETTING));
            _blobClient = _storageAccount.CreateCloudBlobClient();
            Trace.WriteLine("CloudStorage client initialized.");
        }

        public static CloudStorageAccount StorageAccount
        {
            get { return (_storageAccount); }
        }

        public static CloudBlobClient BlobClient
        {
            get { return (_blobClient); }
        }
    }
}