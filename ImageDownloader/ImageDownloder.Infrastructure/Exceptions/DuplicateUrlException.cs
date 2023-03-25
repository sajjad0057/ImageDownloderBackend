namespace ImageDownloder.Infrastructure.Exceptions
{
    public class DuplicateUrlException : Exception
    {
        public DuplicateUrlException(string message) : base(message) { }
    }
}
