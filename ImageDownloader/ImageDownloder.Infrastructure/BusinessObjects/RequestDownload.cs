namespace ImageDownloder.Infrastructure.BusinessObjects
{
    public class RequestDownload
    {
        public IEnumerable<string> ImageUrls { get; set; }
        public int MaxDownloadAtOnce { get; set; }
        public Queue<List<string>> BatchQueue { get; set;} = new Queue<List<string>>();

        public Queue<List<string>> GetImagesUrlQueue()
        {

            if(ImageUrls is not null) 
            {
                ImageUrls.Select((value, index) => new { Index = index, Value = value })
                    .GroupBy(x => x.Index / MaxDownloadAtOnce)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList()
                    .ForEach(batch => BatchQueue.Enqueue(batch));

            }

            return BatchQueue;
        }
    }
}
