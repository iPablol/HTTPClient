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
		private IPAddress address = IPAddress.IsLoopback(address) ? IPAddress.Loopback : address;
		private int port = port;

		private TcpClient client = new();
		private NetworkStream? stream;

		public bool connected
		{
			get
			{
				return client.Connected;
			}
		}

		public async Task<bool> Connect()
		{
			try
			{
				await client.ConnectAsync(new IPEndPoint(address, port));
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

		public async Task<string> Read()
		{
			var buffer = new byte[1_024];
			int received = await stream.ReadAsync(buffer);

			string message = Encoding.UTF8.GetString(buffer, 0, received);
			return message;
		}

		public void Disconnect()
		{
			client?.Close();
			stream?.Close();
			Console.WriteLine("Connection closed");
		}

		public async Task Write(string message) => await stream.WriteAsync(Encoding.UTF8.GetBytes(message));
	}

}
