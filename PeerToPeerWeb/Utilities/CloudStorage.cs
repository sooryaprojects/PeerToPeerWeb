using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeerToPeerWeb.Utilities
{
    public class CloudStorage
    {
        private string _bucketName;
        private string _projectId;
        public CloudStorage(string bucketName, string projectId)
        {
            _bucketName = bucketName;
            _projectId = projectId;
        }

        /// <summary>
        /// Upload Object the Google Cloud Storage Bucket
        /// </summary>
        /// <returns></returns>
        public async Task UploadObject(string objectName, string localPath, string contentType)
        {
            try
            {
                var storage = await StorageClient.CreateAsync();
                using (var fileStream = File.OpenRead(localPath))
                {
                    storage.UploadObject(_bucketName, objectName, contentType, fileStream);
                }
            }catch(Exception e)
            {
                Console.WriteLine($"Following error occured while uploading to Google Cloud Storage: {e.Message}");
            }
            
        }

        /// <summary>
        /// Download Objects from Google Cloud Storage
        /// </summary>
        /// <returns></returns>
        public async Task DownloadObject(string objectName, string localPath)
        {
            try
            {
                var storage = await StorageClient.CreateAsync();
                using (var outputFile = File.OpenWrite(localPath))
                {
                    storage.DownloadObject(_bucketName, objectName, outputFile);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Following error occured while downloading the object: {e.Message}");
            }
           

        }


    }
}
