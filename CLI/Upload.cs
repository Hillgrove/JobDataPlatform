using DataTransfer;
using Google.Apis.Auth.OAuth2;

namespace CLI
{
    public static class Upload
    {
        public static async Task Run(GoogleCredential credential, string localPath, string bucketName, string gcsPrefix)
        {
            Console.WriteLine($"Uploader til BigQuery...");
            await GcsUploader.UploadAllFilesAsync(credential, localPath, bucketName, gcsPrefix);
            Console.WriteLine("upload færdig.\n");
        }
    }
}
