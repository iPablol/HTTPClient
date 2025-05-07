
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
		private static void Send(Command command) => HTTPClient.connection?.Write(command.BuildArgs());

		private static (string method, Uri) Get(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("GET", uri);
		}

		private static (string method, Uri, string body) Post(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("POST", uri, command.BuildArgs(skip: 1));
		}

		private static void ChangeKey(Command command) => HTTPClient.key = command.args.First();

		// Might be interesting to add aliases
		private static readonly Dictionary<string, Delegate> commandDictionary = new()
		{
			//{ "connect", Connect }, // Should be included inside HTTP methods
			{ "quit", Quit },
			{ "bye", Quit },
			//{ "write", Send }, // Should be separated into the HTTP methods
			{ "get", Get },
			{ "post", Post },
			{ "key", ChangeKey }
		};
	}
}
