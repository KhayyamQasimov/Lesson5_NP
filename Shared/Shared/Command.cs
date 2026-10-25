using System.Net.Http;

namespace Shared.Shared
{
    public class Command
    {
        public HttpMethods Method { get; set; }
        public int? Id { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public Car? Car { get; set; }
    }
}