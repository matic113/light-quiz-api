using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
    }
}