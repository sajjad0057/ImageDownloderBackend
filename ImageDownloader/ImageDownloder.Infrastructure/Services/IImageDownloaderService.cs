using ImageDownloder.Infrastructure.BusinessObjects;

namespace ImageDownloder.Infrastructure.Services
{
    public interface IImageDownloaderService
    {
        Task<IDictionary<string,string>> DownloadImageAsync(RequestDownload requestDownload);

        Task<string> GetImageByNameAsync(string imgName);
    }
}
