using System.Net.Sockets;
using System.Net;
using System.Text;

namespace HTTPClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test();
            while (true)
            {

            }
        }

        static async void Test()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 83);

            using TcpClient client = new();
            await client.ConnectAsync(ipEndPoint);
            await using NetworkStream stream = client.GetStream();

            var buffer = new byte[1_024];
            int received = await stream.ReadAsync(buffer);

            var message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Message received: \"{message}\"");

        }
    }
}
