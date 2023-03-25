namespace ImageDownloader.Api.Commons
{
    public sealed class ResponseDownload
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IDictionary<string, string> UrlAndNames { get; set; }

        public static ResponseDownload SuccessResponse(IDictionary<string, string> urlAndNames,string msg) => new()
        {
            Success = true,
            Message = msg,
            UrlAndNames = urlAndNames
        };

        public static ResponseDownload FailedResponse(string msg) => new()
        {
            Success = false,
            Message = msg,
            UrlAndNames = default,
        };
    }
}
