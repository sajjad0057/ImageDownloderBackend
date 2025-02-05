using ImageDownloder.Infrastructure.BusinessObjects;
using System.Net;
using System.Drawing;
using System.Collections.Generic;


namespace ImageDownloder.Infrastructure.Services
{
    public class ImageDownloaderService : IImageDownloaderService
    {
        private readonly HttpClient _httpClient;
        private IDictionary<string, string> _Dict { get; set; } = new Dictionary<string, string>();

        public ImageDownloaderService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IDictionary<string, string>> DownloadImageAsync(RequestDownload requestDownload)
        {
            var queue = new Queue<string>(requestDownload.ImageUrls);
            using var throttler = new SemaphoreSlim(requestDownload.MaxDownloadAtOnce);
            var tasks = new List<Task>();

            var startTime = DateTime.Now;

            while (queue.Count > 0 || tasks.Count > 0)
            {
                if (queue.Count > 0 && tasks.Count < requestDownload.MaxDownloadAtOnce)
                {
                    var url = queue.Dequeue();
                    await throttler.WaitAsync();

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            if (!_Dict.ContainsKey(url))
                            {
                                var response = await _httpClient.GetAsync(url);

                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    var bytes = await response.Content.ReadAsByteArrayAsync();
                                    var imgName = await _SaveImagesAsync(bytes);

                                    if (!string.IsNullOrWhiteSpace(imgName))
                                    {
                                        lock (_Dict) // Ensure thread safety, as if does not access multiple thread at a time
                                        {
                                            _Dict[url] = imgName;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error downloading {url}: {ex.Message}");
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    });

                    tasks.Add(task);
                }

                //// This means that the code will remove all completed tasks from the tasks list.               
                tasks.RemoveAll(t => t.IsCompleted);

                await Task.Delay(100); //// Small delay to avoid CPU overuse
            }

            await Task.WhenAll(tasks);

            var endTime = DateTime.Now;
            var diff = endTime - startTime;
            Console.WriteLine($"Needing total time(Millisecond) to download imege : {diff.TotalMilliseconds}");

            return _Dict;
        }

        private async Task<string> _SaveImagesAsync(byte[] bytes)
        {
            string imgExt;

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (Image img = Image.FromStream(ms))
                {
                    imgExt = img.RawFormat.ToString();
                }
            }

            var rootPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent;

            var folderPath = Path.Combine(rootPath.FullName, "Downloaded_images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var imgName = "";

            if (!string.IsNullOrWhiteSpace(imgExt))
            {
                imgName = string.Concat(Guid.NewGuid().ToString(), $".{imgExt}");

                await File.WriteAllBytesAsync($"{folderPath}\\{imgName}", bytes);
            }

            return imgName;
        }

        public async Task<string> GetImageByNameAsync(string imgName)
        {
            var rootPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent;

            var imgPath = Path.Combine(rootPath.FullName, $"Downloaded_images\\{imgName}");

            if (File.Exists(imgPath))
            {
                return await _ImageToBase64StringAsync(imgPath);
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<string> _ImageToBase64StringAsync(string path)
        {
            byte[] imageBytes = await File.ReadAllBytesAsync(path);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
