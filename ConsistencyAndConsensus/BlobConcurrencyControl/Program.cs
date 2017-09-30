using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobAttributes
{
    class Program
    {
        const string ImageToUpload = "HelloWorld.png";

        static void Main(string[] args)
        {

            var connectionString = Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString");
            
            //Parse the connection string for the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            //Create the service client object for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");

            // Create the container if it does not already exist.
            container.CreateIfNotExists();

            CloudBlockBlob blobReference = container.GetBlockBlobReference(ImageToUpload);
            blobReference.UploadFromFile(ImageToUpload);

            OptimsticConurrency(blobReference);
            PessimisticConurrency(blobReference);

            Console.WriteLine();
        }

        private static void PessimisticConurrency(CloudBlockBlob blockBlob)
        {
            string lease = blockBlob.AcquireLease(TimeSpan.FromSeconds(15), null);

            // Update blob using lease. This operation will succeed
            var accessCondition = AccessCondition.GenerateLeaseCondition(lease);
            blockBlob.UploadText("update", accessCondition: accessCondition);

            try
            {
                // Below operation will fail as no valid lease provided
                blockBlob.UploadText("Update without lease, will fail");
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                    Console.WriteLine("Blob's lease does not match");
                else
                    throw;
            }

        }

        private static void OptimsticConurrency(CloudBlockBlob blockBlob)
        {
            string helloText = "Hello World";
            string orignalETag = blockBlob.Properties.ETag;

            //explictly changing the ETag
            blockBlob.UploadText(helloText+"v1");

            try
            {
                blockBlob.UploadText(helloText,
                    accessCondition: AccessCondition.GenerateIfMatchCondition(orignalETag));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    Console.WriteLine("Blob's orignal etag no longer matches");

                }
                else
                    throw;
            }
        }
    }
}
