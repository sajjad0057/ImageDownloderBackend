using ImageDownloder.Infrastructure.BusinessObjects;
using System.IO;
using System.Net;

namespace ImageDownloder.Infrastructure.Services
{
    public class ImageDownloaderService : IImageDownloaderService
    {
        private readonly HttpClient _httpClient;
        private IDictionary<string, string> _dict { get; set; } = new Dictionary<string, string>();

        public ImageDownloaderService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }


        public async Task<IDictionary<string, string>> DownloadImageAsync(RequestDownload requestDownload)
        {
            var queue = requestDownload.GetImagesUrlQueue();

            IDictionary<string, string> dict = new Dictionary<string, string>();

            var tasks = new List<Task>();

            using var throttler = new SemaphoreSlim(requestDownload.MaxDownloadAtOnce);

            while(queue.Count > 0)
            {
                foreach (var url in queue.Dequeue())
                {
                    await throttler.WaitAsync();

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var response = await _httpClient.GetAsync(url);

                            response.EnsureSuccessStatusCode();

                            if(response.StatusCode == HttpStatusCode.OK)
                            {
                                var bytes = await response.Content.ReadAsByteArrayAsync();

                                var rootPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent;
  
                                var folderPath = Path.Combine(rootPath.FullName, "images");

                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                }

                                Console.WriteLine($"folderPath : {folderPath}");

                                var imgName = Guid.NewGuid().ToString() + ".jpg";

                                Console.WriteLine($"image name : {imgName}");

                                await Task.Delay(3000);

                                Console.WriteLine($"image name> : {imgName}");

                                await File.WriteAllBytesAsync($"{folderPath}\\{imgName}", bytes);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }

            return dict;
        }
    }
}
