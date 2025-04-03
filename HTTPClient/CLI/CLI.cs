
using System.Net;


namespace HTTPClient.CLI
{
	internal static class CLI
	{
		public static CommandResult HandleCommand(Command command)
		{
			if (commandDictionary.TryGetValue(command.keyword, out Delegate? function))
			{
				return new(command.keyword, function.DynamicInvoke(command));
			}
			if (command.keyword == "") return default;
			Console.WriteLine("Invalid command");
			return default;
		}

		private static TCPConnection? Connect(Command command)
		{
			try
			{
				IPAddress? address = Utils.ResolveHost(command.args.First());
				int port = int.Parse(command.args[1]);
				return address is null ? null : new TCPConnection(address, port);
			}
			catch
			{
				return null;
			}
		}

		private static void Quit(Command command) => Environment.Exit(0);

		// Send a generic message
		private static void Send(Command command) => Program.client.connection?.Write(command.BuildArgs());

		private static (string method, Uri, string head) Get(Command command)
		{
			try
			{
				Uri uri = new(command.args.First());
				return ("GET", uri, command.BuildArgs(1));
			}
			catch (UriFormatException ex)
			{
				Console.WriteLine("Invalid URL");
				return default;
			}
			catch (Exception ex)
			{
				return default;
			}
		}

		// Might be interesting to add aliases
		private static readonly Dictionary<string, Delegate> commandDictionary = new()
		{
			//{ "connect", Connect }, // Should be included inside HTTP methods
			{ "quit", Quit },
			{ "bye", Quit },
			//{ "write", Send }, // Should be separated into the HTTP methods
			{ "get", Get }
		};
	}
}
