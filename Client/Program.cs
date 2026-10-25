using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using Shared.Shared;

namespace Client;

class Program
{
    static BinaryWriter bw = null!;
    static BinaryReader br = null!;

    static void Main()
    {
        var client = new TcpClient("127.0.0.1", 27001);
        var stream = client.GetStream();

        bw = new BinaryWriter(stream);
        br = new BinaryReader(stream);

        while (true)
        {
            Console.WriteLine("\n1 - Get all cars");
            Console.WriteLine("2 - Get car by id");
            Console.WriteLine("3 - Add car");
            Console.WriteLine("4 - Update car");
            Console.WriteLine("5 - Delete car");
            Console.WriteLine("0 - Exit");

            var command = new Command();

            switch (Read("Choice"))
            {
                case "1":
                    command.Method = HttpMethods.GET;
                    break;
                case "2":
                    command.Method = HttpMethods.GET;
                    command.Id = ReadInt("Id");
                    break;
                case "3":
                    command.Method = HttpMethods.POST;
                    command.Car = ReadCar(0);
                    break;
                case "4":
                    command.Method = HttpMethods.PUT;
                    command.Car = ReadCar(ReadInt("Id"));
                    break;
                case "5":
                    command.Method = HttpMethods.DELETE;
                    command.Id = ReadInt("Id");
                    break;
                case "0":
                    return;
                default:
                    continue;
            }

            var response = Send(command);

            Console.WriteLine($"\n{response.Message}");
            foreach (var car in response.Cars)
            {
                Console.WriteLine($"{car.Id} | {car.Make} | {car.Model} | {car.Year} | {car.VIN} | {car.Color}");
            }
        }
    }

    static Response Send(Command command)
    {
        bw.Write(JsonSerializer.Serialize(command));
        return JsonSerializer.Deserialize<Response>(br.ReadString())!;
    }

    static string Read(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine() ?? string.Empty;
    }

    static int ReadInt(string label)
    {
        int.TryParse(Read(label), out var value);
        return value;
    }

    static Car ReadCar(int id)
    {
        return new Car
        {
            Id = id,
            Make = Read("Make"),
            Model = Read("Model"),
            Year = (ushort)ReadInt("Year"),
            VIN = Read("VIN"),
            Color = Read("Color")
        };
    }
}