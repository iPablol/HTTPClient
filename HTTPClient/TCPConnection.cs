using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HTTPClient
{
    public class TCPConnection(IPAddress address, int port)
    {
        private IPAddress address = address;
        private int port = port;

        private TcpClient client = new();
        private NetworkStream? stream;

        public async Task<bool> Connect()
        {
            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 83);

                await client.ConnectAsync(new IPEndPoint(this.address, this.port));
                stream = client.GetStream();
                Console.WriteLine($"Opened connection to {address.ToString()} in port {port}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> Read()
        {
            var buffer = new byte[1_024];
            Console.WriteLine("Reading buffer...");
            int received = await stream.ReadAsync(buffer);

            var message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Message received: \"{message}\"");
            return true;
        }

        public void Disconnect()
        {
            client?.Close();
            stream?.Close();
        }

        public async void Write(string message) 
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(message));
        }
    }

}
