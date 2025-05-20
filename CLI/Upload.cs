using DataTransfer;

namespace CLI
{
    public static class Upload
    {
        public static async Task Run(string localPath, string bucketName, string gcsPrefix)
        {
            Console.WriteLine($"Uploader til BigQuery...");
            await GcsUploader.UploadAllFilesAsync(localPath, bucketName, gcsPrefix);
            Console.WriteLine("upload færdig.\n");
        }
    }
}
