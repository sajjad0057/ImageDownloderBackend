namespace ImageDownloader.Api.Commons
{
    public class ResponseGet<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }

        public static ResponseGet<T> SuccessResponse(T data, string msg) => new()
        {
            Success = true,
            Message = msg,
            Data = data
        };

        public static ResponseGet<T> FailedResponse(string msg) => new()
        {
            Success = false,
            Message = msg,
            Data = default(T)
        };
    }
}
