using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABCRetail.Services
{
    public class FileStorageService
    {
        private readonly ShareClient _shareClient;

        public FileStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _shareClient = new ShareClient(
                connectionString,
                "logs");
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            await _shareClient.CreateIfNotExistsAsync();

            ShareDirectoryClient rootDirectory =
                _shareClient.GetRootDirectoryClient();

            string fileName =
                $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

            ShareFileClient fileClient =
                rootDirectory.GetFileClient(fileName);

            using Stream stream = file.OpenReadStream();

            await fileClient.CreateAsync(stream.Length);

            await fileClient.UploadRangeAsync(
                new HttpRange(0, stream.Length),
                stream);
        }

        public async Task<List<string>> GetFilesAsync()
        {
            await _shareClient.CreateIfNotExistsAsync();

            ShareDirectoryClient rootDirectory =
                _shareClient.GetRootDirectoryClient();

            var files = new List<string>();

            await foreach (ShareFileItem item
                in rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }

        public async Task<(Stream Content, string ContentType)>
            DownloadFileAsync(string fileName)
        {
            ShareDirectoryClient rootDirectory =
                _shareClient.GetRootDirectoryClient();

            ShareFileClient fileClient =
                rootDirectory.GetFileClient(fileName);

            ShareFileDownloadInfo download =
                await fileClient.DownloadAsync();

            string extension =
                Path.GetExtension(fileName).ToLower();

            string contentType = extension switch
            {
                ".txt" => "text/plain",
                ".log" => "text/plain",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            return (
                download.Content,
                contentType
            );
        }
    }
}