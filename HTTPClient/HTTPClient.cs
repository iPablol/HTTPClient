using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Command = HTTPClient.CLI.Command;
using CLI = HTTPClient.CLI.CLI;
using HTTPClient.CLI;
using System.Text.RegularExpressions;

namespace HTTPClient
{
	public class HTTPClient
	{
		public bool connected
		{
			get
			{
				return connection?.connected ?? false;
			}
		}

		private const string httpVersion = "HTTP/1.1";

		public TCPConnection? connection { get; private set; } = null;


		public void Run()
		{
			try
			{
				while (true)
				{
					CommandResult result = CLI.CLI.HandleCommand(new(Console.ReadLine() ?? ""));
					Task<bool>? successful = null;
					switch (result.result)
					{
						case (string method, Uri url):
							successful = Request(method, url, "", "");
							break;
						case (string method, Uri url, string body):
							successful = Request(method, url, "", body);
							break;
						case (string method, Uri url, string head, string body):
							successful = Request(method, url, head, body);
							break;
						default:
							
							break;
					}
					if (successful is null) continue;
					successful.Wait();
					if (successful.Result)
					{
						Task<string> response = AwaitResponse();
						response.Wait();
						HandleResponse(response);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex}");
			}
			finally
			{
				connection?.Disconnect();
			}
		}

		private async Task<bool> Request(string method, Uri url, string head, string body)
		{
			if (url.Scheme != "http")
			{
				Console.WriteLine("Unsuported protocol");
				return false;
			}
			if (await Connect(url))
			{
				await connection.Write($"{method} {GetTarget(url)} {httpVersion}\r\n{InjectHeaders(head, body)}\r\n{body}");
				return true;
			}
			return false;
		}

		private async Task<bool> Connect(Uri url)
		{
			IPAddress? address = Utils.ResolveHost(url.Host);
			if (address is null) { Console.WriteLine("Error connecting to host"); return false; }
			int port = url.Port;
			connection = new TCPConnection(address, port);
			return await connection.Connect();
		}


		private async Task<string> AwaitResponse()
		{
			string responseMessage = await connection.Read();
			return responseMessage;
		}

		private void HandleResponse(Task<string> message)
		{
			var response = ParseResponse(message.Result);
			if (response.code.StartsWith('4') || response.code.StartsWith('5'))
			{
				Console.WriteLine($"{response.code}, {response.message}");
			}
            else
            {
				Console.WriteLine(response.body); 
            }
			connection?.Disconnect();
		}

		private (string version, string code, string message, string[] headers, string body) ParseResponse(string response)
		{
			Regex regex = new("(.*) ([1-5][0-9][0-9]) (.*)\r\n(.*)\r\n(.*)");
			try
			{
				if (regex.IsMatch(response))
				{
					var matches = regex.Match(response).Groups.Values.Select(x => x.Value).ToArray();
					return (matches[1], matches[2], matches[3], matches[4].Split('\n'), matches[5]);
				}
			}
			catch
			{ }
			throw new FormatException("Response was malformatted");
		}

		private string InjectHeaders(string head, string body = "")
		{
			StringBuilder sb = new(head);
			sb.AppendLine("Host: localhost");
			sb.AppendLine("User-Agent: custom/1.0");
			sb.AppendLine("Accept: */*");
			if (body.Length > 0)
			{
				sb.AppendLine("Content-Type: application/json");
				sb.AppendLine($"Content-Length: {body.Length}");
			}

			return sb.ToString();
		}

		private string GetTarget(Uri url) => form switch
		{
			TARGET_FORM.ORIGIN => url.AbsolutePath,
			TARGET_FORM.ABSOLUTE => url.ToString(),
			TARGET_FORM.AUTHORITY => $"{url.Host}:{url.Port}",
			TARGET_FORM.ASTERISK => "*",
			_ => "/"
		};

		private enum TARGET_FORM
		{
			ORIGIN,
			ABSOLUTE,
			AUTHORITY,
			ASTERISK
		}

		private TARGET_FORM form = TARGET_FORM.ORIGIN;
	}
}
