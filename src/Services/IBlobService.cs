namespace light_quiz_api.Services
{
    public interface IBlobService
    {
        Task<string> UploadBlobAsync(string containerName, string blobName, byte[] content, string contentType);
        Task DeleteBlobAsync(string containerName, string blobName);
    }
}
