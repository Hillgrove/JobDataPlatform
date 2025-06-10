using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace DataTransfer
{
    public class GcsUploader
    {
        public static async Task UploadAllFilesAsync(GoogleCredential credential, string localFolder, string bucketName, string gcsFolder = "")
        {
            var client = await StorageClient.CreateAsync(credential);
            
            var files = Directory.GetFiles(localFolder, "*.*", SearchOption.AllDirectories);

            var uploadTasks = files.Select(async file =>
            {
                var relativePath = Path.GetRelativePath(localFolder, file).Replace("\\", "/");
                var objectName = string.IsNullOrEmpty(gcsFolder) ? relativePath : $"{gcsFolder}/{relativePath}";
                
                using var stream = File.OpenRead(file);
                await client.UploadObjectAsync(bucketName, objectName, null, stream);

                Console.WriteLine($"Uploadet: {objectName}");
            });

            await Task.WhenAll(uploadTasks);
        }
    }
}
