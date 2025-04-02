using System.Net.Sockets;
using System.Net;
using System.Text;

namespace HTTPClient
{
    internal class Program
    {
        private static TCPConnection connection = new(IPAddress.Loopback, 83);

        static void Main(string[] args)
        {
            Run();
            string command = "";
            try
            {
                while (command != "quit")
                {
                    command = Console.ReadLine() ?? "";
                    if (command == "read")
                    {
                        connection.Read();
                    }
                    if (command.Split(' ')[0] == "write")
                    {
                        connection.Write(command.Substring(command.IndexOf(' ') + 1));
                    }
                }
            }
            finally
            {
                connection.Disconnect();
            }
        }


        static async void Run()
        {
            bool connected = await connection.Connect();
        }
    }
}
