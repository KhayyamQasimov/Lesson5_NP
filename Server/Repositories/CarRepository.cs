using Shared.Shared;

namespace Server.Repositories
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        public CarRepository(string filePath) : base(filePath, "Cars")
        {
        }
    }
}