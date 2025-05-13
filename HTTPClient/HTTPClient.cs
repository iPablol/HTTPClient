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
	public static class HTTPClient
	{
		public static bool connected
		{
			get
			{
				return connection?.connected ?? false;
			}
		}

		internal static string key = "";

		private const string httpVersion = "HTTP/1.1";

		public static TCPConnection? connection { get; private set; } = null;


		public static void Run()
		{
			while (true)
			{
				try
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
							HandleResponse(response.Result);
						}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
				finally
				{
					connection?.Disconnect();
				}
			}
		}

		private static async Task<bool> Request(string method, Uri url, string head, string body)
		{
			if (url.Scheme != "http")
			{
				Console.WriteLine("Unsuported protocol");
				return false;
			}
			if (await Connect(url))
			{
				// Apparently InjectHeaders adds an extra \r\n
				await connection.Write($"{method} {GetTarget(url)} {httpVersion}\r\n{InjectHeaders(head, body)}\r\n{body}");
				return true;
			}
			return false;
		}

		private static async Task<bool> Connect(Uri url)
		{
			IPAddress? address = Utils.ResolveHost(url.Host);
			if (address is null) { Console.WriteLine("Error connecting to host"); return false; }
			int port = url.Port;
			connection = new TCPConnection(address, port);
			return await connection.Connect();
		}


		private static async Task<string> AwaitResponse()
		{
			string responseMessage = await connection.Read();
			return responseMessage;
		}

		private static void HandleResponse(string message)
		{
			var response = ParseResponse(message);
			Console.WriteLine($"{response.code}, {response.message}");
			Console.WriteLine(response.body);
			//if (response.code.StartsWith('4') || response.code.StartsWith('5'))
			//{
			//}
			//else
			//{
			//}
			connection?.Disconnect();
		}

		private static (string version, string code, string message, string[] headers, string body) ParseResponse(string response)
		{
			Regex regex = new("(.*) ([1-5][0-9][0-9]) (.*)\r\n((?:[^\\r\\n]+\\r\\n)*?)\\r\\n(.*)");
			try
			{
				if (regex.IsMatch(response))
				{
					var matches = regex.Match(response).Groups.Values.Select(x => x.Value).ToArray();
					return (matches[1], matches[2], matches[3], matches[4].Split("\r\n"), matches[5]);
				}
			}
			catch
			{ }
			throw new FormatException("Response was malformatted");
		}

		private static string InjectHeaders(string head, string body = "")
		{
			StringBuilder sb = new(head);
			sb.AppendLine("Host: localhost");
			sb.AppendLine("User-Agent: custom/1.0");
			sb.AppendLine("Accept: */*");
			// https://www.rfc-editor.org/rfc/rfc6648#section-3 prefixing headers with 'X-' is discouraged
			if (key != "") sb.AppendLine($"Key: {key}");
			if (body.Length > 0)
			{
				sb.AppendLine("Content-Type: application/json");
				sb.AppendLine($"Content-Length: {Encoding.UTF8.GetBytes(body).Length}");
			}

			return sb.ToString();
		}

		private static string GetTarget(Uri url) => form switch
		{
			TARGET_FORM.ORIGIN => url.AbsolutePath,
			TARGET_FORM.ABSOLUTE => url.ToString(),
			TARGET_FORM.AUTHORITY => $"{url.Host}:{url.Port}",
			TARGET_FORM.ASTERISK => "*",
			_ => "/"
		};

		// https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/Messages#http2_messages
		private enum TARGET_FORM
		{
			ORIGIN,
			ABSOLUTE,
			AUTHORITY,
			ASTERISK
		}

		private static TARGET_FORM form = TARGET_FORM.ORIGIN;
	}
}
