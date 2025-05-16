using Google.Cloud.Storage.V1;

namespace Load
{
    public class GcsUploader
    {
        public static async Task UploadAllFilesAsync(string localFolder, string bucketName, string gcsFolder = "")
        {
            var keyPath = Path.Combine(AppContext.BaseDirectory, "secrets", "gcs-key.json");
            var client = await StorageClient.CreateAsync(Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(keyPath));
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
