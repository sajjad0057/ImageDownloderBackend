using ImageDownloder.Infrastructure.BusinessObjects;
using System.Net.Http;

namespace ImageDownloder.Infrastructure.Services
{
    public class ImageDownloaderService : IImageDownloaderService
    {
        private readonly HttpClient _httpClient;
        public ImageDownloaderService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        public async Task<IDictionary<string, string>> DownloadImageAsync(RequestDownload requestDownload)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("url", "imageName");
            var tasks = new List<Task>();
            using var throttler = new SemaphoreSlim(3);
            foreach (var url in requestDownload.ImageUrls)
            {
                await throttler.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        var bytes = await response.Content.ReadAsByteArrayAsync();

                        var rootPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent;
                        var folderPath = Path.Combine(rootPath.FullName, "images");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var imgName = Guid.NewGuid().ToString() + ".jpg";

                        Console.WriteLine($"image name : {imgName}");

                        await Task.Delay(3000);

                        Console.WriteLine($"image name> : {imgName}");

                        File.WriteAllBytes($"{folderPath}\\{imgName}", bytes);
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
            

            return dict;
        }
    }
}
