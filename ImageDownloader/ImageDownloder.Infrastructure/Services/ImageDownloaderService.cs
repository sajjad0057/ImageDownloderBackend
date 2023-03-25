using ImageDownloder.Infrastructure.BusinessObjects;
using System.Net;
using System.Drawing;
using ImageDownloder.Infrastructure.Exceptions;


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
            var queue = requestDownload.GetImagesUrlQueue();

            using var throttler = new SemaphoreSlim(requestDownload.MaxDownloadAtOnce);

            while (queue.Count > 0)
            {
                var tasks = new List<Task>();

                foreach (var url in queue.Dequeue())
                {
                    await throttler.WaitAsync();

                    tasks.Add(Task.Run(async () =>
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
                                    try
                                    {
                                        _Dict.Add(url, imgName);

                                    }
                                    catch (Exception ex)
                                    {
                                        throw new DuplicateUrlException($"Duplicate download image url not acceptable ! {ex.Message}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new DuplicateUrlException("Duplicate download image url not acceptable !");
                        }

                        throttler.Release();

                    }));
                }

                await Task.WhenAll(tasks);
            }

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
