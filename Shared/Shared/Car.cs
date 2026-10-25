using System.Security.Principal;

namespace Shared.Shared
{
    public class Car : IEntity
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public ushort Year { get; set; }
        public string VIN { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}