namespace OMarket.Domain.Exceptions.App
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() : base("Access is forbidden.")
        { }

        public ForbiddenAccessException(string message) : base(message)
        { }

        public ForbiddenAccessException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}