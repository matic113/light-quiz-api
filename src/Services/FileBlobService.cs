using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
namespace light_quiz_api.Services
{
    public class FileBlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string reportsContainerName = "reports";
        private const string picturesContainerName = "pictures";
        public FileBlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadBlobAsync(string containerName, string blobName, byte[] content, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = new MemoryStream(content))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            }

            return blobClient.Uri.ToString();
        }
        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
        public async Task<string> UploadReportAsync(string reportName, byte[] content)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; // excel files
            return await UploadBlobAsync(reportsContainerName, reportName, content, contentType);
        }
        public async Task<string> UploadPictureAsync(string pictureName, byte[] content)
        {
            string contentType = "image/jpeg"; // jpeg files
            return await UploadBlobAsync(picturesContainerName, pictureName, content, contentType);
        }
        public Uri GeneratePictureUploadSasUri(string pictureName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(picturesContainerName);

            var blobClient = containerClient.GetBlobClient(pictureName);

            // Define the permissions for the SAS.
            // 'Create' allows writing a new blob, 'Write' allows writing to an existing blob.
            // For uploads, you typically need Create and Write.
            BlobSasPermissions permissions = BlobSasPermissions.Create | BlobSasPermissions.Write;

            // Set the expiry time for the SAS.
            // It's crucial to set a short expiry time for security.
            // For example, 15 minutes from now.
            DateTimeOffset expiryTime = DateTimeOffset.UtcNow.AddMinutes(15);

            // Create a BlobSasBuilder object
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = picturesContainerName,
                BlobName = pictureName,
                ExpiresOn = expiryTime,
                // Optional: Specify allowed IP address or range for added security
                // IPRange = new Azure.Storage.Sas.SasIPRange(System.Net.IPAddress.Parse("YOUR_CLIENT_IP_ADDRESS")),
                // Optional: Require HTTPS (highly recommended)
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(permissions);

            // Generate the SAS URI.
            // The BlobServiceClient is initialized with the connection string (which contains the account key),
            // so it can sign the SAS.
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri;
        }
    }
}