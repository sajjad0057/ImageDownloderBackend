namespace ImageDownloader.Api.Models
{
    public class RequestDownloadModel
    {
        public IEnumerable<string> ImageUrls { get; set; }
        public int MaxDownloadAtOnce { get; set; }
    }

}
