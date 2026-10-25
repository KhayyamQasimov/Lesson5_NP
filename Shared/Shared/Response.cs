namespace Shared.Shared
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Car> Cars { get; set; } = new();
    }
}