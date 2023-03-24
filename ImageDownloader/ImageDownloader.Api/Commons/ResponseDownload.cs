namespace ImageDownloader.Api.Commons
{
    public sealed class ResponseDownload
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IDictionary<string, string> UrlAndNames { get; set; }

        public static ResponseDownload SuccessResponse(IDictionary<string, string> urlAndNames) => new()
        {
            Success = true,
            Message = "Images are downloaded successfully !",
            UrlAndNames = urlAndNames
        };


        public static ResponseDownload FailedResponse(IDictionary<string, string> urlAndNames) => new()
        {
            Success = false,
            Message = "There have a problem in download image",
            UrlAndNames = default,
        };
    }
}
