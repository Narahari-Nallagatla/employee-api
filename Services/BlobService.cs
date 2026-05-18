using Azure.Storage.Blobs;

namespace EmployeeApi.Services
{
    public class BlobService
    {
        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
            var containerName = _configuration["AzureBlobStorage:ContainerName"];

            var blobContainerClient =
                new BlobContainerClient(connectionString, containerName);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var blobClient =
                blobContainerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(stream);

            return blobClient.Uri.ToString();
        }
    }
}