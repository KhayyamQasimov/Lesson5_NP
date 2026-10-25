using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using Server.Repositories;
using Shared.Shared;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "Cars.json");
            ICarRepository carRepository = new CarRepository(filePath);

            var tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 27001);
            tcpListener.Start(10);

            Console.WriteLine($"{tcpListener.Server.LocalEndPoint} listening.....");

            while (true)
            {
                var tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} connected server....");

                Task.Run(() => HandleClient(tcpClient, carRepository));
            }
        }

        static void HandleClient(TcpClient tcpClient, ICarRepository carRepository)
        {
            using (tcpClient)
            {
                var stream = tcpClient.GetStream();
                var br = new BinaryReader(stream);
                var bw = new BinaryWriter(stream);

                try
                {
                    while (true)
                    {
                        var command = JsonSerializer.Deserialize<Command>(br.ReadString());
                        var response = Process(command, carRepository);
                        bw.Write(JsonSerializer.Serialize(response));
                    }
                }
                catch (Exception ex) when (ex is EndOfStreamException || ex is IOException)
                {
                    Console.WriteLine("Client disconnected....");
                }
            }
        }

        static Response Process(Command? command, ICarRepository carRepository)
        {
            if (command == null)
                return new Response { Success = false, Message = "Invalid command" };

            switch (command.Method)
            {
                case HttpMethods.GET:
                    var cars = carRepository.GetAll()
                        .Where(x => (command.Id == null || x.Id == command.Id)
                                 && (command.Make == null || x.Make.Equals(command.Make, StringComparison.OrdinalIgnoreCase))
                                 && (command.Model == null || x.Model.Equals(command.Model, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    return new Response
                    {
                        Success = cars.Count > 0,
                        Message = cars.Count > 0 ? "Cars found" : "No car found",
                        Cars = cars
                    };

                case HttpMethods.POST:
                    if (command.Car == null)
                        return new Response { Success = false, Message = "Car data is missing" };

                    return carRepository.Add(command.Car)
                        ? new Response { Success = true, Message = "Car added successfully" }
                        : new Response { Success = false, Message = "Car could not be added" };

                case HttpMethods.PUT:
                    if (command.Car == null)
                        return new Response { Success = false, Message = "Car data is missing" };

                    return carRepository.Update(command.Car)
                        ? new Response { Success = true, Message = "Car updated successfully" }
                        : new Response { Success = false, Message = "Car not found" };

                case HttpMethods.DELETE:
                    if (command.Id == null)
                        return new Response { Success = false, Message = "Id is missing" };

                    return carRepository.Delete(command.Id.Value)
                        ? new Response { Success = true, Message = "Car deleted successfully" }
                        : new Response { Success = false, Message = "Car not found" };

                default:
                    return new Response { Success = false, Message = "Unknown method" };
            }
        }
    }
}